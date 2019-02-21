using System.Data;
using System.Linq;
using RDotNet;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Base;
using Ria.Calculations.Library.Interfaces;

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
            var dataFrame = engine.Evaluate("result").AsDataFrame();

            DataTable table = new DataTable("Positions");

            //DataGridView 
            //for (int i = 0; i < dataset.ColumnCount; ++i)
            //{
            //    dataGridView1.ColumnCount++;
            //    dataGridView1.Columns[i].Name = dataset.ColumnNames[i];
            //}
 
            //for (int i = 0; i < dataset.RowCount; ++i)
            //{
            //    dataGridView1.RowCount++;
            //    dataGridView1.Rows[i].HeaderCell.Value = dataset.RowNames[i];
 
            //    for (int k = 0; k < dataset.ColumnCount; ++k)
            //    {
            //        dataGridView1[k, i].Value = dataset[i,k];
 
            //    }
 
            //}
            var listOfDates = dataFrame.ElementAt(0).ToList();
            var listOfValues = dataFrame.ElementAt(1).ToList();
            var dic = listOfDates.Zip(listOfValues, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);

            this._dataService.MergeHistoricalPositions();
        }
    }
}
