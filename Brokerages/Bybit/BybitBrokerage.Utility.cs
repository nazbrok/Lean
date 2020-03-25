using System;
namespace QuantConnect.Brokerages.Bybit
{
    public partial class BybitBrokerage
    {
        public readonly DateTime dt1970 = new DateTime(1970, 1, 1);

        public long GetNonce()
        {
            return (DateTime.UtcNow - dt1970).Ticks;
        }
    }
}
