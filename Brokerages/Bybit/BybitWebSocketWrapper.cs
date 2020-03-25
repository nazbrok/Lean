using System;
namespace QuantConnect.Brokerages.Bybit
{
    /// <summary>
    /// Wrapper class for a Bybit websocket connection
    /// </summary>
    public class BytbitWebSocketWrapper : WebSocketWrapper
    {
        /// <summary>
        /// The unique Id for the connection
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// The handler for the connection
        /// </summary>
        public IConnectionHandler ConnectionHandler { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytbitWebSocketWrapper"/> class.
        /// </summary>
        public BytbitWebSocketWrapper(IConnectionHandler connectionHandler)
        {
            ConnectionId = Guid.NewGuid().ToString();
            ConnectionHandler = connectionHandler;
        }
    }
}
