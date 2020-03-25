using System;
namespace QuantConnect.Brokerages.Bybit
{
    public class BybitSubscriptionManager
    {
        private readonly BybitBrokerage _brokerage;
        private readonly string _wssUrl;
        private readonly BybitSymbolMapper _symbolMapper;

        public BybitSubscriptionManager(BybitBrokerage brokerage, string wssUrl, BybitSymbolMapper symbolMapper)
        {
            _brokerage = brokerage;
            _wssUrl = wssUrl;
            _symbolMapper = symbolMapper;
        }
    }
}
