using QuantConnect.Securities;

namespace QuantConnect.Orders.Fees
{
    /// <summary>
    /// Bybit fee model
    /// </summary>
    public class BybitFeeModel: FeeModel
    {
        /// <summary>
        /// Maker fees
        /// Maker fees are paid when you add liquidity to our order book by placing a limit order under the ticker price for buy and above the ticker price for sell.
        /// https://help.bybit.com/hc/en-us/articles/360039261154-Trading-fee-calculation
        /// </summary>
        public const decimal MakerFee = -0.0025m;

        /// <summary>
        /// Taker fees
        /// Taker fees are paid when you remove liquidity from our order book by placing any order that is executed against an order of the order book.
        /// Note: If you place a hidden order, you will always pay the taker fee. If you place a limit order that hits a hidden order, you will always pay the maker fee.
        /// https://help.bybit.com/hc/en-us/articles/360039261154-Trading-fee-calculation
        /// </summary>
        public const decimal TakerFee = 0.0075m;

        // <summary>
        /// Get the fee for this order in quote currency
        /// </summary>
        /// <param name="parameters">A <see cref="OrderFeeParameters"/> object
        /// containing the security and order</param>
        /// <returns>The cost of the order in quote currency</returns>
        public override OrderFee GetOrderFee(OrderFeeParameters parameters)
        {
            var order = parameters.Order;
            var security = parameters.Security;
            decimal fee = TakerFee;
            var props = order.Properties as BybitOrderProperties;

            if (order.Type == OrderType.Limit &&                
                (props?.PostOnly == true || !order.IsMarketable))
            {
                // limit order posted to the order book
                fee = MakerFee;
            }

            // get order value in quote currency
            var unitPrice = order.Direction == OrderDirection.Buy ? security.AskPrice : security.BidPrice;
            if (order.Type == OrderType.Limit)
            {
                // limit order posted to the order book
                unitPrice = ((LimitOrder)order).LimitPrice;
            }

            unitPrice *= security.SymbolProperties.ContractMultiplier;

            // apply fee factor, currently we do not model 30-day volume, so we use the first tier
            return new OrderFee(new CashAmount(
                unitPrice * order.AbsoluteQuantity * fee,
                security.QuoteCurrency.Symbol));
        }
    }
}
