using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDotNet;
using Ria.CalculationEngine.Processors.Interface;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Base;
using Ria.Calculations.Library.Events;
using Ria.Calculations.Library.Interfaces;
using Ria.Calculations.Library.Model;

namespace Ria.Calculations.Library.Implementation
{
    public class CashFlowBasedCalculations : CalculationBase, ICashFlowBasedCalculations, IDisposable
    {
        private readonly IDataService _dataService;
        private readonly IProducerConsumer<CashFlowBasedItem> _producerConsumer;

        public CashFlowBasedCalculations(IDataService dataService, IProducerConsumer<CashFlowBasedItem> producerConsumer)
        {
            _dataService = dataService;
            _producerConsumer = producerConsumer;

            _producerConsumer.ProcessBatchedItems += OnProcessBatchedItems;
        }

        private void OnProcessBatchedItems(List<CashFlowBasedItem> cashFlowBatchedItems)
        {
            // Now send the batched items to the database probably via some sort of SqlBulkCopy
            this._dataService.MergeCashFlows();
        }

        public override void Calculate(REngine engine)
        {
            this.MergeCashFlows(engine);
            //this.MergeXirr(engine);
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

            DateTime epoch = new DateTime(1970,1,1);

            int days_since_epoch = 15791;

            DateTime converted = epoch.AddDays(days_since_epoch);

            List<DateTime> dateTimes = new List<DateTime>();
            foreach (var item in dataFrame.ElementAt(0))
            {
                int dayNo;
                int.TryParse(item.ToString(), out dayNo);
                dateTimes.Add(epoch.AddDays(dayNo));
            }

            var listOfDates = dateTimes;
            var listOfValues = dataFrame.ElementAt(1).ToList();

            var dic = listOfDates.Zip(listOfValues, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);

            BlockingCollection<CashFlowBasedItem> blockingCollection = new BlockingCollection<CashFlowBasedItem>();

            Parallel.ForEach(dic, (currentItem) =>
            {
                CashFlowBasedItem cfItem = new CashFlowBasedItem();
                cfItem.Date = (DateTime) currentItem.Key;
                cfItem.Cashflow = (double) currentItem.Value;
                blockingCollection.Add(cfItem);
            });
            
            this._producerConsumer.Start(blockingCollection);
            



        }

        public void Dispose()
        {
            Cleanup(true);
            GC.SuppressFinalize(this);
        }
        private void Cleanup(bool disposing)
        {
            if (!disposing)
            {
                _producerConsumer.ProcessBatchedItems -= this.OnProcessBatchedItems;
            }

        }

        public void Finalize()
        {
            Cleanup(false);

        }
    }
}
