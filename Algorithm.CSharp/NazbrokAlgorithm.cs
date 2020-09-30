using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    public class NazbrokAlgorithm : QCAlgorithm
    {
        private NazbrokSymbolDataParameter _parameters;

        private Dictionary<Symbol, NazbrokSymbolData> _data;

        public override void Initialize()
        {
            SetStartDate(2018, 04, 04);
            SetEndDate(2018, 04, 06);

            SetCash(1000);

            //SetBrokerageModel(Brokerages.BrokerageName.GDAX, AccountType.Cash);

            _data = new Dictionary<Symbol, NazbrokSymbolData>();

            _parameters = new NazbrokSymbolDataParameter()
            {
                VolumeWeightedAveragePricePeriod = 10,
            };

            var crypto = AddCrypto("BTCUSD", Resolution.Minute, Market.GDAX);

            _data.Add(crypto.Symbol, new NazbrokSymbolData(this, crypto, _parameters));

        }

        private decimal _price = 0.0m;

        private bool _hasPosition = false;

        public override void OnData(Slice slice)
        {
            foreach(var symbolData in _data)
            {
                var localSymbol = symbolData.Key;
                var localData = symbolData.Value;

                var bar = slice.Bars[localSymbol];

                localData.Update(bar);

                if (!localData.IsReady())
                {
                    continue;
                }

                if (Portfolio[localSymbol].Invested)
                {
                    if (bar.Close < _price)
                    {
                        Liquidate(localSymbol);
                    }

                    if (bar.Close < localData.Wwap.Current)
                    {
                        Liquidate(localSymbol);
                    }
                }
                else
                {
                    if (bar.Close > localData.Wwap.Current)
                    {
                        SetHoldings(localSymbol, 0.9);
                    }
                }
            }

        }

        public override void OnEndOfAlgorithm()
        {
            Debug($"AccountCurrency = {Portfolio.CashBook.AccountCurrency}");
            Debug($"TotalValueInAccountCurrency = {Portfolio.CashBook.TotalValueInAccountCurrency}");
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status == OrderStatus.Filled)
            {
                _price = orderEvent.FillPrice;
            }
        }
    }

    public class NazbrokSymbolDataParameter
    {
        public int VolumeWeightedAveragePricePeriod { get; set; }
    }


    public class NazbrokSymbolData
    {
        private QCAlgorithm _algorithm;
        private NazbrokSymbolDataParameter _parameters;
        public Security Security { get; private set; }

        public VolumeWeightedAveragePriceIndicator Wwap { get; private set; }

        public NazbrokSymbolData(QCAlgorithm algorithm, Security security, NazbrokSymbolDataParameter parameters)
        {
            _algorithm = algorithm;
            Security = security;
            _parameters = parameters;

            Wwap = new VolumeWeightedAveragePriceIndicator(_parameters.VolumeWeightedAveragePricePeriod);
        }


        public bool IsReady()
        {
            return Wwap.IsReady;
        }

        public void Update(TradeBar input)
        {
            Wwap.Update(input);
        }
    }

}
