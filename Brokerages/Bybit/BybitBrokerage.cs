using QuantConnect.Orders;
using QuantConnect.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Brokerages.Bybit
{
    public partial class BybitBrokerage : BaseWebsocketsBrokerage
    {
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

        public override void Subscribe(IEnumerable<Symbol> symbols)
        {
            throw new NotImplementedException();
        }

        public override bool UpdateOrder(Order order)
        {
            throw new NotImplementedException();
        }
    }
}
