using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp.Nazbrok
{
    /// <summary>
    /// Uses Wilder's RSI to create insights. Using default settings, a cross over below 30 or above 70 will
    /// trigger a new insight.
    /// </summary>
    public class NazbrokAlphaModel : AlphaModel
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
        /// Initializes a new instance of the <see cref="NazbrokAlphaModel"/> class
        /// </summary>
        /// <param name="period">The RSI indicator period</param>
        /// <param name="resolution">The resolution of data sent into the RSI indicator</param>
        public NazbrokAlphaModel(
            int tenkanPeriod = 9, 
            int kijunPeriod = 26, 
            int senkouAPeriod = 26, 
            int senkouBPeriod= 52, 
            int senkouADelayPeriod, 
            int senkouBDelayPeriod, 
            Resolution resolution = Resolution.Daily
            )
        {
            _tenkanPeriod = tenkanPeriod;
            _kijunPeriod = kijunPeriod;
            _senkouAPeriod = senkouAPeriod;
            _senkouBPeriod = senkouBPeriod;
            _senkouADelayPeriod = senkouADelayPeriod;
            _senkouBDelayPeriod = senkouBDelayPeriod;
            _resolution = resolution
            Name = $"{nameof(NazbrokAlphaModel)}({_period},{_resolution})";
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
                var state = GetState(ichimoku, previousState);

                if (state != previousState && ichimoku.IsReady)
                {
                    var insightPeriod = _resolution.ToTimeSpan().Multiply(_period);

                    switch (state)
                    {
                        case State.TrippedLow:
                            insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Up));
                            break;

                        case State.TrippedHigh:
                            insights.Add(Insight.Price(symbol, insightPeriod, InsightDirection.Down));
                            break;
                    }
                }

                kvp.Value.State = state;
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
                algorithm.History(addedSymbols, _period, _resolution)
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

        /// <summary>
        /// Determines the new state. This is basically cross-over detection logic that
        /// includes considerations for bouncing using the configured bounce tolerance.
        /// </summary>
        private State GetState(IchimokuKinkoHyo ichimoku, State previous)
        {
            ichimoku.KijunMaximum

            vwap.Current.
            if (rsi > 70m)
            {
                return State.TrippedHigh;
            }

            if (rsi < 30m)
            {
                return State.TrippedLow;
            }

            if (previous == State.TrippedLow)
            {
                if (rsi > 35m)
                {
                    return State.Middle;
                }
            }

            if (previous == State.TrippedHigh)
            {
                if (rsi < 65m)
                {
                    return State.Middle;
                }
            }

            return previous;
        }

        /// <summary>
        /// Contains data specific to a symbol required by this model
        /// </summary>
        private class SymbolData
        {
            public Symbol Symbol { get; }
            public State State { get; set; }

            public IchimokuKinkoHyo ICHIMOKU { get; }

            public SymbolData(Symbol symbol, IchimokuKinkoHyo ichimoku)
            {
                Symbol = symbol;
                ICHIMOKU = ichimoku;
                State = State.Middle;
            }
        }

        /// <summary>
        /// Defines the state. This is used to prevent signal spamming and aid in bounce detection.
        /// </summary>
        private enum State
        {
            TrippedLow,
            Middle,
            TrippedHigh
        }
    }
}
