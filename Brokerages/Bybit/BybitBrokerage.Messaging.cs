using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuantConnect.Interfaces;
using QuantConnect.Logging;
using QuantConnect.Securities;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly ConcurrentQueue<WebSocketMessage> _messageBuffer = new ConcurrentQueue<WebSocketMessage>();
        private volatile bool _streamLocked;
        private readonly BybitSubscriptionManager _subscriptionManager;

        private readonly SymbolPropertiesDatabase _symbolPropertiesDatabase;

        private readonly IAlgorithm _algorithm;

        public BybitBrokerage(string wssUrl, string restUrl, string apiKey, string apiSecret, IAlgorithm algorithm, IPriceProvider priceProvider)
            : this(wssUrl, new WebSocketWrapper(), new RestClient(restUrl), apiKey, apiSecret, algorithm, priceProvider)
        {
        }

        public BybitBrokerage(string wssUrl, IWebSocket websocket, IRestClient restClient, string apiKey, string apiSecret, IAlgorithm algorithm, IPriceProvider priceProvider)
           : base(wssUrl, websocket, restClient, apiKey, apiSecret, Market.Bybit, "Bybit")
        {
            _subscriptionManager = new BybitSubscriptionManager(this, wssUrl, _symbolMapper);
            _symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();
            _algorithm = algorithm;

            WebSocket.Open += (sender, args) =>
            {
                SubscribeAuth();
            };
        }

        /// <summary>
        /// Wss message handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void OnMessage(object sender, WebSocketMessage e)
        {
            LastHeartbeatUtcTime = DateTime.UtcNow;

            // Verify if we're allowed to handle the streaming packet yet; while we're placing an order we delay the
            // stream processing a touch.
            try
            {
                if (_streamLocked)
                {
                    _messageBuffer.Enqueue(e);
                    return;
                }
            }
            catch (Exception err)
            {
                Log.Error(err);
            }

            OnMessageImpl(e);
        }


        private object GetTypeMessage(JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
        }

        /// <summary>
        /// Implementation of the OnMessage event
        /// </summary>
        /// <param name="e"></param>
        private void OnMessageImpl(WebSocketMessage e)
        {
            try
            {
                var token = JToken.Parse(e.Message);

                if (token is JArray)
                {
                    var channel = token[0].ToObject<int>();
                    // heartbeat
                    if (token[1].Type == JTokenType.String && token[1].Value<string>() == "hb")
                    {
                        return;
                    }
                    //public channels
                    if (channel == 0)
                    {
                        var term = token[1].ToObject<string>();
                        switch (term.ToLowerInvariant())
                        {
                            case "oc":
                                //OnOrderClose(token[2].ToObject<string[]>());
                                return;
                            case "tu":
                                //EmitFillOrder(token[2].ToObject<string[]>());
                                return;
                            default:
                                return;
                        }
                    }
                }
                else if (token is JObject)
                {
                    var test = token.ToObject<Messages.Test>();

                    if (test == null)
                    {
                        Log.Trace($"not a Test");
                    }

                    var raw = token.ToObject<Messages.SubscriptionResponse>();
                    switch (raw.Request.Operation.ToLowerInvariant())
                    {
                        case "auth":
                            //var auth = token.ToObject<Messages.AuthResponseMessage>();
                            var result = raw.Success ? "succeed" : "failed";
                            Log.Trace($"BybitWebsocketsBrokerage.OnMessage: Subscribing to authenticated channels {result}");
                            return;
                        case "info":
                        case "ping":
                            return;
                        case "error":
                            //var error = token.ToObject<Messages.ErrorMessage>();
                            //Log.Trace($"BitfinexWebsocketsBrokerage.OnMessage: {error.Level}: {error.Message}");
                            return;
                        default:
                            Log.Trace($"BitfinexWebsocketsBrokerage.OnMessage: Unexpected message format: {e.Message}");
                            break;
                    }

                }
            }
            catch (Exception exception)
            {
                OnMessage(new BrokerageMessageEvent(BrokerageMessageType.Error, -1, $"Parsing wss message failed. Data: {e.Message} Exception: {exception}"));
                throw;
            }
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
            foreach (var symbol in symbols)
            {
                _subscriptionManager.Unsubscribe(symbol);

                Log.Trace($"BybitBrokerage.Unsubscribe(): Sent unsubscribe for {symbol.Value}.");
            }
        }

        private void SubscribeAuth()
        {
            if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(ApiSecret))
            {
                return;
            }

            var authNonce = GetNonce();
            var expires = authNonce + 1000;

            var authPayload = "GET/realtime" + expires;
            var authSignature = AuthenticationToken(authPayload);

            WebSocket.Send(JsonConvert.SerializeObject(new
            {
                op = "auth",
                args = new string[] { ApiKey, expires.ConvertInvariant<string>(), authSignature }

            }));

            Log.Trace("BybitBrokerage.SubscribeAuth() : Sent authentication request.");
        }

        private string AuthenticationToken(string payload)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(ApiSecret)))
            {
                return ByteArrayToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            }
        }

        private string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }

    /// <summary>
    /// Message type
    /// </summary>

    public enum BybitMessageType
    {
        Subscription,
        Topic,
    }
}
