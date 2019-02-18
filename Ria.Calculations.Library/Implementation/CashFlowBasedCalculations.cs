using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDotNet;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Base;
using Ria.Calculations.Library.Events;
using Ria.Calculations.Library.Interfaces;

namespace Ria.Calculations.Library.Implementation
{
    public class CashFlowBasedCalculations : CalculationBase, ICashFlowBasedCalculations
    {
        private readonly IDataService _dataService;

        public CashFlowBasedCalculations(IDataService dataService)
        {
            _dataService = dataService;
        }
        public override void Calculate(REngine engine)
        {
            this.MergeCashFlows(engine);
            this.MergeXirr(engine);
        }

        // Fires something to tell that we've merged
        public Action<CalculationNotificationEvent> NotificationEvent { get; set; }

        /// <summary>
        /// If this is a time intensive operation we'd probably parallelize this
        /// so that we can have multiple tasks calculating each ticker.
        /// </summary>
        /// <param name="engine"></param>
        private void MergeXirr(REngine engine)
        {
            engine.Evaluate("cashflowList <- c(cashFlows$coredata)");
            engine.Evaluate("dateList <- c(cashFlows$date)");

            engine.Evaluate("rateOfReturn <- xirr(cashFlowList,dateList)");
            var rateOfReturn = engine.Evaluate("rateOfReturn").AsNumeric()[0];

            this._dataService.MergeXirr();

            // TODO - this is nonsensical at present - we'll have to have a valid ENUM or something to signify an
            // event identifier we know about.
            this.NotificationEvent?.Invoke(new CalculationNotificationEvent("We've merged Xirr", Guid.NewGuid()));
        }


        /// <summary>
        /// Same goes for this.
        /// </summary>
        /// <param name="engine"></param>
        private void MergeCashFlows(REngine engine)
        {
            engine.Evaluate("FCF = get.fund.data('free cash flow', fund, fund.date)");
            engine.Evaluate("cashFlows <- data.frame(date=index(FCF), coredata(FCF))");
            var dataFrame = engine.Evaluate("cashFlows").AsDataFrame();
            var listOfDates = dataFrame.ElementAt(0).ToList();
            var listOfValues = dataFrame.ElementAt(1).ToList();

            this._dataService.MergeCashFlows();

        }
    }
}
