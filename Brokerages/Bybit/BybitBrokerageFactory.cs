using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Securities;
using QuantConnect.Util;
using System;
using System.Collections.Generic;

namespace QuantConnect.Brokerages.Bybit
{
    public class BybitBrokerageFactory : BrokerageFactory
    {
        /// <summary>
        /// Factory constructor
        /// </summary>
        public BybitBrokerageFactory() : base(typeof(BybitBrokerage))
        {
        }

        public override Dictionary<string, string> BrokerageData => new Dictionary<string, string>
        {
            { "bybit-rest" , Config.Get("bybit-rest", "https://api.bybit.com")},
            { "bybit-url" , Config.Get("bybit-url", "wss://stream.bybit.com/realtime")},
            { "bybit-api-key", Config.Get("bybit-api-key")},
            { "bybit-api-secret", Config.Get("bybit-api-secret")}
        };

        /// <summary>
        /// Create the Brokerage instance
        /// </summary>
        /// <param name="job"></param>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        public override IBrokerage CreateBrokerage(Packets.LiveNodePacket job, IAlgorithm algorithm)
        {
            var required = new[] { "bybit-rest", "bybit-wss", "bybit-api-secret", "bybit-api-key" };

            foreach (var item in required)
            {
                if (string.IsNullOrEmpty(job.BrokerageData[item]))
                    throw new Exception($"BybitBrokerageFactory.CreateBrokerage: Missing {item} in config.json");
            }

            var priceProvider = new ApiPriceProvider(job.UserId, job.UserToken);

            var brokerage = new BybitBrokerage(
                job.BrokerageData["bybit-wss"],
                job.BrokerageData["bybit-rest"],
                job.BrokerageData["bybit-api-key"],
                job.BrokerageData["bybit-api-secret"],
                algorithm,
                priceProvider);
            Composer.Instance.AddPart<IDataQueueHandler>(brokerage);

            return brokerage;
        }

        /// <summary>
        /// Not required
        /// </summary>
        public override void Dispose()
        {

        }

        public override IBrokerageModel GetBrokerageModel(IOrderProvider orderProvider) => new BybitBrokerageModel();
    }
}
