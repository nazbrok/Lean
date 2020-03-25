using QuantConnect.Data;
using QuantConnect.Interfaces;
using QuantConnect.Orders;
using QuantConnect.Packets;
using QuantConnect.Securities;
using System;
using System.Collections.Generic;

namespace QuantConnect.Brokerages.Bybit
{
    public partial class BybitBrokerage : BaseWebsocketsBrokerage, IDataQueueHandler
    {
        private readonly BybitSymbolMapper _symbolMapper = new BybitSymbolMapper();


        /// <summary>
        /// Checks if the websocket connection is connected or in the process of connecting
        /// </summary>
        public override bool IsConnected => WebSocket.IsOpen;

        public override bool CancelOrder(Order order)
        {
            throw new NotImplementedException();
        }

        public override List<Holding> GetAccountHoldings()
        {
            throw new NotImplementedException();
        }

        public override List<CashAmount> GetCashBalance()
        {
            throw new NotImplementedException();
        }

        public override List<Order> GetOpenOrders()
        {
            throw new NotImplementedException();
        }

        public override void OnMessage(object sender, WebSocketMessage e)
        {
            throw new NotImplementedException();
        }

        public override bool PlaceOrder(Order order)
        {
            throw new NotImplementedException();
        }


        public override bool UpdateOrder(Order order)
        {
            throw new NotImplementedException();
        }

        #region IDataQueueHandler

        /// <summary>
        /// Get the next ticks from the live trading data queue
        /// </summary>
        /// <returns>IEnumerable list of ticks since the last update.</returns>
        public IEnumerable<BaseData> GetNextTicks()
        {
            lock (TickLocker)
            {
                var copy = Ticks.ToArray();
                Ticks.Clear();
                return copy;
            }
        }

        /// <summary>
        /// Adds the specified symbols to the subscription
        /// </summary>
        /// <param name="job">Job we're subscribing for:</param>
        /// <param name="symbols">The symbols to be added keyed by SecurityType</param>
        public void Subscribe(LiveNodePacket job, IEnumerable<Symbol> symbols)
        {
            Subscribe(symbols);
        }

        /// <summary>
        /// Removes the specified symbols to the subscription
        /// </summary>
        /// <param name="job">Job we're processing.</param>
        /// <param name="symbols">The symbols to be removed keyed by SecurityType</param>
        public void Unsubscribe(LiveNodePacket job, IEnumerable<Symbol> symbols)
        {
            Unsubscribe(symbols);
        }

        #endregion
    }
}
