using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace QuantConnect.Algorithm.CSharp.Nazbrok
{
    /// <summary>
    /// Uses Wilder's RSI to create insights. Using default settings, a cross over below 30 or above 70 will
    /// trigger a new insight.
    /// </summary>
    public class NazbrokIchimokuAlphaModel : AlphaModel
    {
        private readonly Dictionary<Symbol, SymbolData> _symbolDataBySymbol = new Dictionary<Symbol, SymbolData>();

        private readonly int _tenkanPeriod;
        private readonly int _kijunPeriod;
        private readonly int _senkouAPeriod;
        private readonly int _senkouBPeriod;
        private readonly int _senkouADelayPeriod;
        private readonly int _senkouBDelayPeriod;

        private readonly Resolution _resolution;

        /// <summary>
        /// Initializes a new instance of the <see cref="NazbrokIchimokuAlphaModel"/> class
        /// </summary>
        /// <param name="period">The RSI indicator period</param>
        /// <param name="resolution">The resolution of data sent into the RSI indicator</param>
        public NazbrokIchimokuAlphaModel(
            int tenkanPeriod = 9, 
            int kijunPeriod = 26, 
            int senkouAPeriod = 26, 
            int senkouBPeriod = 52, 
            int senkouADelayPeriod = 26, 
            int senkouBDelayPeriod = 26, 
            Resolution resolution = Resolution.Daily
            )
        {
            _tenkanPeriod = tenkanPeriod;
            _kijunPeriod = kijunPeriod;
            _senkouAPeriod = senkouAPeriod;
            _senkouBPeriod = senkouBPeriod;
            _senkouADelayPeriod = senkouADelayPeriod;
            _senkouBDelayPeriod = senkouBDelayPeriod;
            _resolution = resolution;
            Name = $"{nameof(NazbrokIchimokuAlphaModel)}({_tenkanPeriod},{_kijunPeriod},{_senkouAPeriod},{_senkouBPeriod},{_senkouADelayPeriod},{_senkouBDelayPeriod},{_resolution})";
        }

        [Conditional("DEBUG")]
        private void DebugInfo(QCAlgorithm algorithm, Symbol symbol, TradeBar qb, IchimokuCloudLocation ichimokuLocation)
        {
            algorithm.Log($"Time : {qb.Time} Symbol : {symbol.Value} location : {ichimokuLocation.ToString()}");
        }

        /// <summary>
        /// Updates this alpha model with the latest data from the algorithm.
        /// This is called each time the algorithm receives data for subscribed securities
        /// </summary>
        /// <param name="algorithm">The algorithm instance</param>
        /// <param name="data">The new data available</param>
        /// <returns>The new insights generated</returns>
        public override IEnumerable<Insight> Update(QCAlgorithm algorithm, Slice data)
        {
            var insights = new List<Insight>();

            foreach (var kvp in _symbolDataBySymbol)
            {
                var symbol = kvp.Key;
                var symbolData = kvp.Value;
                //var previousState = kvp.Value.State;
                TradeBar qb = null;
                if (data.Bars.TryGetValue(symbol, out qb))
                {
                    // Update indicator with the latest TradeBar
                    symbolData.Update(qb);

                    // Determine insight direction
                    var currentCloudLocation = symbolData.GetCloudLocation();
                    if (symbolData.PreviousCloudLocation != null)
                    {
                        if (symbolData.PreviousCloudLocation != IchimokuCloudLocation.Above &&  currentCloudLocation == IchimokuCloudLocation.Above)
                        {
                            symbolData.Direction = InsightDirection.Up;
                        }

                        if (symbolData.PreviousCloudLocation != IchimokuCloudLocation.Under && currentCloudLocation == IchimokuCloudLocation.Under)
                        {
                            symbolData.Direction = InsightDirection.Down;
                        }
                    }

                    symbolData.PreviousCloudLocation = currentCloudLocation;

                    // Emit insight
                    if (symbolData.Direction.HasValue)
                    {
                        var insight = Insight.Price(symbolData.Symbol, symbolData.Resolution, 1, symbolData.Direction.Value);
                        insights.Add(insight);
                    }
                }
            }

            return insights;
        }

        /// <summary>
        /// Cleans out old security data and initializes the RSI for any newly added securities.
        /// This functional also seeds any new indicators using a history request.
        /// </summary>
        /// <param name="algorithm">The algorithm instance that experienced the change in securities</param>
        /// <param name="changes">The security additions and removals from the algorithm</param>
        public override void OnSecuritiesChanged(QCAlgorithm algorithm, SecurityChanges changes)
        {
            // clean up data for removed securities
            if (changes.RemovedSecurities.Count > 0)
            {
                var removed = changes.RemovedSecurities.ToHashSet(x => x.Symbol);
                foreach (var subscription in algorithm.SubscriptionManager.Subscriptions)
                {
                    if (removed.Contains(subscription.Symbol))
                    {
                        _symbolDataBySymbol.Remove(subscription.Symbol);
                        subscription.Consolidators.Clear();
                    }
                }
            }

            // initialize data for added securities
            var addedSymbols = new List<Symbol>();
            foreach (var added in changes.AddedSecurities)
            {
                if (!_symbolDataBySymbol.ContainsKey(added.Symbol))
                {
                    var symbolData = new SymbolData(added.Symbol, algorithm, _tenkanPeriod, _kijunPeriod, _senkouAPeriod, _senkouBPeriod, _senkouADelayPeriod, _senkouBDelayPeriod, _resolution);
                    _symbolDataBySymbol[added.Symbol] = symbolData;
                    addedSymbols.Add(symbolData.Symbol);
                }
            }

            if (addedSymbols.Count > 0)
            {
                // warmup our indicators by pushing history through the consolidators
                algorithm.History(addedSymbols, GetPeriod(), _resolution)
                    .PushThrough(data =>
                    {
                        SymbolData symbolData;
                        if (_symbolDataBySymbol.TryGetValue(data.Symbol, out symbolData))
                        {
                            symbolData.Update(data);
                        }
                    });
            }
        }

        private int GetPeriod()
        {
            return Math.Max(_tenkanPeriod, Math.Max(_kijunPeriod, Math.Max(_senkouADelayPeriod, Math.Max(_senkouBDelayPeriod, Math.Max(_senkouAPeriod, _senkouBPeriod)))));
        }

        /// <summary>
        /// Contains data specific to a symbol required by this model
        /// </summary>
        private class SymbolData
        {
            #region Properties
            public Resolution Resolution { get; }

            public Symbol Symbol { get; }

            public QCAlgorithm Algorithm { get; }

            public IchimokuCloudLocation? PreviousCloudLocation { get; set; }

            public InsightDirection? Direction { get; set; }

            public IchimokuKinkoHyo ICHIMOKU { get; }

            #endregion

            public SymbolData(Symbol symbol, QCAlgorithm algorithm, int tenkanPeriod = 9, int kijunPeriod = 26, int senkouAPeriod = 26, int senkouBPeriod = 52, int senkouADelayPeriod = 26, int senkouBDelayPeriod = 26, Resolution resolution = Resolution.Daily)
            {
                Symbol = symbol;
                Algorithm = algorithm;
                Resolution = resolution;
                
                ICHIMOKU = algorithm.ICHIMOKU(symbol, tenkanPeriod, kijunPeriod, senkouAPeriod, senkouBPeriod, senkouADelayPeriod, senkouBDelayPeriod, resolution);
            }

            public IchimokuCloudLocation GetCloudLocation()
            {
                var chikou = ICHIMOKU.Chikou.Current.Value;

                var senkou_span_a = ICHIMOKU.SenkouA.Current.Value;
                var senkou_span_b = ICHIMOKU.SenkouB.Current.Value;
                var cloudTop = Math.Max(senkou_span_a, senkou_span_b);
                var cloudBottom = Math.Min(senkou_span_a, senkou_span_b);

                if (chikou > cloudTop)
                {
                    return IchimokuCloudLocation.Above;
                }
                else if (chikou < cloudBottom)
                {
                    return IchimokuCloudLocation.Under;
                }
                else
                {
                    return IchimokuCloudLocation.Inside;
                }
            }

            public void Update(BaseData data)
            {
                if (data == null || data.Symbol != Symbol)
                {
                    return;
                }

                ICHIMOKU.Update(data);
            }
        }

        /// <summary>
        /// Defines the state. This is used to prevent signal spamming and aid in bounce detection.
        /// </summary>
        public enum IchimokuCloudLocation
        {
            Under = -1,
            
            Inside = 0,

            Above = 1,
        }
    }
}
