using QuantConnect.Configuration;
using QuantConnect.Interfaces;
using QuantConnect.Packets;
using QuantConnect.Securities;
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

        public override IBrokerage CreateBrokerage(LiveNodePacket job, IAlgorithm algorithm)
        {
            throw new NotImplementedException();
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
