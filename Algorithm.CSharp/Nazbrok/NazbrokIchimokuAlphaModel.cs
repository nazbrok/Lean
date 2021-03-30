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
        private void DebugInfo(QCAlgorithm algorithm, Symbol symbol, TradeBar qb, IchimokuState state)
        {
            algorithm.Log($"Time : {qb.Time} Symbol : {symbol.Value} Clound {state.CloudState}");
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
                var ichimoku = kvp.Value.ICHIMOKU;
                var previousState = kvp.Value.State;
                TradeBar qb = null;
                if (data.Bars.TryGetValue(symbol, out qb))
                {
                    var state = GetState(ichimoku, previousState, qb);

                    if (ichimoku.IsReady && previousState.CloudState != state.CloudState)
                    {
                        var insightPeriod = _resolution.ToTimeSpan().Multiply(_tenkanPeriod);
                        if (state.CloudState == IchimokuCloudState.Above)
                        {
                            insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Up));
                            DebugInfo(algorithm, symbol, qb, state);
                        }
                        if (state.CloudState == IchimokuCloudState.Under)
                        {
                            insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Down));
                            DebugInfo(algorithm, symbol, qb, state);
                        }
                    }

                    kvp.Value.State = state;
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
                    var ichimoku = algorithm.ICHIMOKU(added.Symbol, _tenkanPeriod, _kijunPeriod, _senkouAPeriod, _senkouBPeriod, _senkouADelayPeriod, _senkouBDelayPeriod, _resolution);
                    var symbolData = new SymbolData(added.Symbol, ichimoku);
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
                            symbolData.ICHIMOKU.Update(data.EndTime, data.Value);
                        }
                    });
            }
        }

        private int GetPeriod()
        {
            return Math.Max(_tenkanPeriod, Math.Max(_kijunPeriod, Math.Max(_senkouADelayPeriod, Math.Max(_senkouBDelayPeriod, Math.Max(_senkouAPeriod, _senkouBPeriod)))));
        }

        /// <summary>
        /// Determines the new state. This is basically cross-over detection logic that
        /// includes considerations for bouncing using the configured bounce tolerance.
        /// </summary>
        private IchimokuState GetState(IchimokuKinkoHyo ichimoku, IchimokuState previous, TradeBar data)
        {
            var symbol = data.Symbol;

            var state = new IchimokuState();

            SymbolData symbolData = null;
            _symbolDataBySymbol.TryGetValue(symbol, out symbolData);

            if (symbolData != null)
            {
                if (data.Close > symbolData.ICHIMOKU.SenkouA && data.Close > symbolData.ICHIMOKU.SenkouB)
                {
                    state.CloudState = IchimokuCloudState.Above;
                }
                else if (data.Close > symbolData.ICHIMOKU.SenkouA || data.Close > symbolData.ICHIMOKU.SenkouB)
                {
                    state.CloudState = IchimokuCloudState.Inside;
                }
                else
                {
                    state.CloudState = IchimokuCloudState.Under;
                }
            }

            return state;
        }

        /// <summary>
        /// Contains data specific to a symbol required by this model
        /// </summary>
        private class SymbolData
        {
            public Symbol Symbol { get; }
            
            public IchimokuState State { get; set; }

            public IchimokuKinkoHyo ICHIMOKU { get; }

            public SymbolData(Symbol symbol, IchimokuKinkoHyo ichimoku)
            {
                State = new IchimokuState();
                Symbol = symbol;
                ICHIMOKU = ichimoku;
                State.CloudState = IchimokuCloudState.Inside;
            }
        }

        private class IchimokuState
        {
            public IchimokuCloudState CloudState { get; set; }

            public IchimokuState()
            {
                CloudState = IchimokuCloudState.Inside;
            }
        }

        /// <summary>
        /// Defines the state. This is used to prevent signal spamming and aid in bounce detection.
        /// </summary>
        private enum IchimokuCloudState
        {
            Under,
            
            Inside,

            Above,
        }

        //private enum Ichimoku


    }
}
