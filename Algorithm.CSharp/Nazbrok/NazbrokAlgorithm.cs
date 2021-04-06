using QuantConnect.Algorithm.CSharp.Nazbrok;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Execution;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Algorithm.Framework.Risk;
using QuantConnect.Algorithm.Framework.Selection;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantConnect.Algorithm.CSharp
{
    public class NazbrokAlgorithmParameters
    {
        private const string CST_AmountCash = "amount-cash";
        private const string CST_TickerSymbol = "ticker-symbol";
        private const string CST_Ichimoku_Tenkan = "ichimoku-tenkan";
        private const string CST_Ichimoku_Kijun = "ichimoku-kijun";
        private const string CST_Ichimoku_SenkouSpanA = "ichimoku-senkou-span-a";
        private const string CST_Ichimoku_SenkouSpanB = "ichimoku-senkou-span-b";
        private const string CST_Ichimoku_ChikouSpan = "ichimoku-chikou-span";


        private QCAlgorithm _algorithm;

        public NazbrokAlgorithmParameters(QCAlgorithm algorithm)
        {
            _algorithm = algorithm;
        }

        public decimal AmountCash => _algorithm.GetParameter(CST_AmountCash).ToDecimal();

        public string Ticker => _algorithm.GetParameter(CST_TickerSymbol);

        public int IchimokuTenkan => _algorithm.GetParameter(CST_Ichimoku_Tenkan).ToInt32();
        public int IchimokuKenjun => _algorithm.GetParameter(CST_Ichimoku_Kijun).ToInt32();
        public int IchimokuSenkouSpanA => _algorithm.GetParameter(CST_Ichimoku_SenkouSpanA).ToInt32();
        public int IchimokuSenkouSpanB => _algorithm.GetParameter(CST_Ichimoku_SenkouSpanB).ToInt32();
        public int IchimokuChikouSpan => _algorithm.GetParameter(CST_Ichimoku_ChikouSpan).ToInt32();
    }

    public class NazbrokAlgorithm: QCAlgorithm
    {
        #region Variables

        private string _ticker;

        private NazbrokAlgorithmParameters _parameters;

        #endregion

        private void InitializeAlgoParams()
        {
            _ticker = _parameters.Ticker;
            
            SetBrokerageModel(Brokerages.BrokerageName.Bitfinex, AccountType.Margin);
            //            SetBrokerageModel(Brokerages.BrokerageName.Bitfinex);

            UniverseSettings.Resolution = Resolution.Hour;
            SetUniverseSelection(new ManualUniverseSelectionModel(QuantConnect.Symbol.Create(_ticker, SecurityType.Crypto, Market.Bitfinex)));

            SetAlpha(new NazbrokIchimokuAlphaModel(_parameters.IchimokuTenkan, _parameters.IchimokuKenjun, _parameters.IchimokuSenkouSpanA, _parameters.IchimokuSenkouSpanB, _parameters.IchimokuChikouSpan, _parameters.IchimokuChikouSpan, UniverseSettings.Resolution));
            SetPortfolioConstruction(new EqualWeightingPortfolioConstructionModel());
            SetExecution(new ImmediateExecutionModel());
            SetRiskManagement(new TrailingStopRiskManagementModel(0.05m));
        }

        private void InitializeBacktesParams()
        {
            DebugMode = true;

            SetStartDate(2021, 1, 1); // Set Start Date
            SetEndDate(2021, 2, 28); // Set End Date

            SetCash(_parameters.AmountCash);

            SetWarmUp(120);
        }



        public override void Initialize()
        {
            _parameters = new NazbrokAlgorithmParameters(this);

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
