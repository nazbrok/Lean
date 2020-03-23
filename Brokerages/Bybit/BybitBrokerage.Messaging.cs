using QuantConnect.Interfaces;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Bybit
{
    public partial class BybitBrokerage
    {
        public BybitBrokerage(string wssUrl, string restUrl, string apiKey, string apiSecret, IAlgorithm algorithm, IPriceProvider priceProvider)
            : this(wssUrl, new WebSocketWrapper(), new RestClient(restUrl), apiKey, apiSecret, algorithm, priceProvider)
        {
        }

        public BybitBrokerage(string wssUrl, IWebSocket websocket, IRestClient restClient, string apiKey, string apiSecret, IAlgorithm algorithm, IPriceProvider priceProvider)
           : base(wssUrl, websocket, restClient, apiKey, apiSecret, Market.Bybit, "Bybit")
        {
            //_subscriptionManager = new BitfinexSubscriptionManager(this, wssUrl, _symbolMapper);
            //_symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();
            //_algorithm = algorithm;

            //WebSocket.Open += (sender, args) =>
            //{
            //    SubscribeAuth();
            //};
        }
    }
}
