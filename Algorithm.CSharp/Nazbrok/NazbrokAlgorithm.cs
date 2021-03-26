using QuantConnect.Algorithm.CSharp.Nazbrok;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    public class NazbrokAlgorithm: QCAlgorithm
    {
        #region Constantes

        private const string CST_AmountCash = "amount-cash";
        private const string CST_TickerSymbol = "ticker-symbol";

        #endregion

        #region Fields
        private string  _ticker;
        #endregion

        private void InitializeAlgoParams()
        {
            _ticker = GetParameter(CST_TickerSymbol);

            SetBrokerageModel(Brokerages.BrokerageName.Bitfinex, AccountType.Margin);
            //            SetBrokerageModel(Brokerages.BrokerageName.Bitfinex);

            SetUniverseSelection(new ManualUniverseSelectionModel(QuantConnect.Symbol.Create(_ticker, SecurityType.Crypto, Market.Bitfinex)));

            SetAlpha(new NazbrokAlphaModel(resolution: Resolution.Hour));
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel());
            SetExecution(new ImmediateExecutionModel());
            SetRiskManagement(new TrailingStopRiskManagementModel(0.02m));
        }


        private void InitializeBacktesParams()
        {
            DebugMode = true;

            SetStartDate(2021, 1, 1); // Set Start Date
            SetEndDate(2021, 2, 28); // Set End Date

            SetCash(GetParameter(CST_AmountCash).ToDecimal());
        }



        public override void Initialize()
        {
            InitializeAlgoParams();
            InitializeBacktesParams();
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            if (orderEvent.Status.IsFill())
            {
                Log($"{Time}: {orderEvent.Symbol} Order filled: {orderEvent.Quantity} / {orderEvent.OrderFee} Holding : {Securities[_ticker].Holdings.Quantity} Net Profit: {Securities[_ticker].Holdings.NetProfit}");
            }
        }

    }
}
