using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using RDotNet;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Base;
using Ria.Calculations.Library.Interfaces;
using Ria.Model.Model;

namespace Ria.Calculations.Library.Implementation
{
    public class InvestmentBasedCalculations : CalculationBase, IInvestmentBasedCalculations
    {
        private readonly IDataService _dataService;

        private object _syncLock = new object();

        public InvestmentBasedCalculations(IDataService dataService)
        {
            _dataService = dataService;
        }

        public override void Calculate(REngine engine)
        {
            lock (_syncLock)
            {
                this.MergeTotalEquities(engine);
                this.MergeInvestedCapital(engine);
            }
        }

        private void MergeInvestedCapital(REngine engine)
        {
            // JS - Improvement substitute in the string in the R to do the function evaluation
            // JS - actually refactor this as we are doing the same thing multiple times.
            engine.Evaluate("ICP = get.fund.data('invested capital', fund, fund.date)");
            engine.Evaluate("result <- data.frame(date=as.character(index(ICP)), coredata(ICP))");
            
            var dataFrame = engine.Evaluate("result").AsDataFrame();
            var listOfDates = dataFrame.ElementAt(0).ToList();
            var listOfValues = dataFrame.ElementAt(1).ToList();
            var dic = listOfDates.Zip(listOfValues, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);

            this._dataService.MergeInvestedCapital();
        }

        private void MergeGrowthRates(REngine engine)
        {
            engine.Evaluate("growthRate <- FCF/ICP");
            engine.Evaluate("result <- data.frame(date=as.character(index(growthRate)), coredata(growthRate))");
            var dataFrame = engine.Evaluate("result").AsDataFrame();
            var listOfDates = dataFrame.ElementAt(0).ToList();
            var listOfValues = dataFrame.ElementAt(1).ToList();
            var dic = listOfDates.Zip(listOfValues, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);
        }

        

        /// <summary>
        /// which probably needs to work out and query the r-data based only on the date i.e we will have
        /// already calculated the holdings historically - so just add the new ones.
        /// </summary>
        /// <param name="engine"></param>
        private void MergeTotalEquities(REngine engine)
        {
            //#Delete all dates with no prices

            engine.Evaluate("portfolioPrices <- portfolioPrices[apply(portfolioPrices,1,function(x) all(!is.na(x))),]");
            engine.Evaluate("colnames(portfolioPrices) <- tickers");

            engine.Evaluate("result <- data.frame(date=as.character(index(portfolioPrices)), coredata(portfolioPrices))");
            
            var reshaped = engine.Evaluate("reshaped <-melt(result, id.vars = 1:2)").AsDataFrame();

            List<PriceLineItem> pricesList = new List<PriceLineItem>();

            for(int i = 1; i < reshaped.RowCount; ++i)
            {
                var row = reshaped.GetRow(i);
                var ticker = row[2].ToString();
                double outPrice;
                double.TryParse(row[3].ToString(), out outPrice);
                DateTime outDate;
                DateTime.TryParse(row[0].ToString(), out outDate);
                
                var priceLineItem = new PriceLineItem(outDate, ticker, outPrice);
                pricesList.Add(priceLineItem);
            }
            
            this._dataService.MergePrices(pricesList);
        }
    }


}
