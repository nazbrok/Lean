using QuantConnect.Interfaces;
using QuantConnect.Logging;
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
        /// <summary>
        /// Locking object for the Ticks list in the data queue handler
        /// </summary>
        public readonly object TickLocker = new object();

        private readonly BybitSubscriptionManager _subscriptionManager;

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

        /// <summary>
        /// Subscribes to the requested symbols (using an individual streaming channel)
        /// </summary>
        /// <param name="symbols">The list of symbols to subscribe</param>
        public override void Subscribe(IEnumerable<Symbol> symbols)
        {
            /* foreach (var symbol in symbols)
             {
                 if (_subscriptionManager.IsSubscribed(symbol) ||
                     symbol.Value.Contains("UNIVERSE") ||
                     !_symbolMapper.IsKnownBrokerageSymbol(symbol.Value) ||
                     symbol.SecurityType != _symbolMapper.GetLeanSecurityType(symbol.Value))
                 {
                     continue;
                 }

                 _subscriptionManager.Subscribe(symbol);

                 Log.Trace($"BitfinexBrokerage.Subscribe(): Sent subscribe for {symbol.Value}.");
             } */
        }

        /// <summary>
        /// Ends current subscriptions
        /// </summary>
        public void Unsubscribe(IEnumerable<Symbol> symbols)
        {
            /*  foreach (var symbol in symbols)
              {
                  _subscriptionManager.Unsubscribe(symbol);

                  Log.Trace($"BybitBrokerage.Unsubscribe(): Sent unsubscribe for {symbol.Value}.");
              }*/
        }
    }
}
