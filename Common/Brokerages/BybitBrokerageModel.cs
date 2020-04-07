using QuantConnect.Orders.Fees;
using QuantConnect.Securities;
using QuantConnect.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Brokerages
{
    /// <summary>
    /// Bybit brokerage model
    /// </summary>
    public class BybitBrokerageModel : DefaultBrokerageModel
    {
        // Max leverage by trading pair.
        private static readonly Dictionary<string, decimal> MaxLeverages = new Dictionary<string, decimal>()
        {
            { "BTCUSD", 100.0m },
            { "ETHUSD", 50.0m },
            { "EOSUSD", 50.0m },
            { "XRPUSD", 50.0m },
        };

        /// <summary>
        /// Gets a map of the default markets to be used for each security type
        /// </summary>
        public override IReadOnlyDictionary<SecurityType, string> DefaultMarkets { get; } = GetDefaultMarkets();

        public BybitBrokerageModel()
            : base(AccountType.Margin)
        {
        }

        /// <summary>
        /// Gets a new buying power model for the security, returning the default model with the security's configured leverage.
        /// </summary>
        /// <param name="security">The security to get a buying power model for</param>
        /// <returns>The buying power model for this brokerage/security</returns>
        public override IBuyingPowerModel GetBuyingPowerModel(Security security)
        {
            if (!MaxLeverages.ContainsKey(security.Symbol.Value))
            {
                throw new ArgumentException($"No leverage defined for the secrutity ${security.Symbol.Value}");
            }

            return new SecurityMarginModel(MaxLeverages[security.Symbol.Value]);
        }

        /// <summary>
        /// Bybit global leverage rule
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public override decimal GetLeverage(Security security)
        {
            if (security == null)
            {
                throw new ArgumentNullException(nameof(security));
            }

            if (security.Type == SecurityType.Crypto)
            {
                decimal leverage;
                if (MaxLeverages.TryGetValue(security.Symbol.Value, out leverage))
                {
                    return leverage;
                }

                throw new ArgumentException($"Invalid security type: {security.Symbol.Value}", nameof(security));
            }

            throw new ArgumentException($"Invalid security type: {security.Type}", nameof(security));
        }

        /// <summary>
        /// Provides Bitfinex fee model
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public override IFeeModel GetFeeModel(Security security)
        {
            return new BybitFeeModel();
        }

        private static IReadOnlyDictionary<SecurityType, string> GetDefaultMarkets()
        {
            var map = DefaultMarketMap.ToDictionary();
            map[SecurityType.Crypto] = Market.Bybit;
            return map.ToReadOnlyDictionary();
        }
    }
}
