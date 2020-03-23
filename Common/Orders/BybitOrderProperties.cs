using QuantConnect.Interfaces;

namespace QuantConnect.Orders
{
    /// <summary>
    /// Bybit order properties
    /// </summary>
    public class BybitOrderProperties: OrderProperties
    {
        /// <summary>
        /// Post only order will not be executed immediately in the market as Taker, using this type of order will ensure
        /// a Maker fee reward.
        /// If the order will be executed immediately, the order will be automatically cancelled
        /// </summary>
        public bool PostOnly { get; set; }

        /// <summary>
        /// A reduce only order will only reduce yours position, not increase it.
        /// If this order would increase your position, it is amended down or canceled such that it does not.
        /// </summary>
        public bool ReduceOnly { get; set; }

        /// <summary>
        /// Returns a new instance clone of this object
        /// </summary>
        public override IOrderProperties Clone()
        {
            return (BybitOrderProperties)MemberwiseClone();
        }
    }
}
