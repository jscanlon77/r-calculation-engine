using System.Linq;
using RDotNet;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Base;
using Ria.Calculations.Library.Interfaces;

namespace Ria.Calculations.Library.Implementation
{
    public class PriceBasedCalculations: CalculationBase, IPriceBasedCalculations
    {
        private readonly IDataService _dataService;

        public PriceBasedCalculations(IDataService dataService)
        {
            _dataService = dataService;
        }

        private void MergePrices(REngine engine)
        {
            engine.Evaluate("prices = Cl(data[[tickers[1]]]['1995::'])");

            engine.Evaluate("res <- data.frame(date=as.character(index(prices)), coredata(prices))");

            var dataFrame = engine.Evaluate("res").AsDataFrame();

            var listOfDates = dataFrame.ElementAt(0).ToList();
            var listOfValues = dataFrame.ElementAt(1).ToList();

            var dic = listOfDates.Zip(listOfValues, (k, v) => new { k, v })
                .ToDictionary(x => x.k, x => x.v);

            // now do a merge of the prices including new prices into the database
            this._dataService.MergePrices();
        }

        public override void Calculate(REngine engine)
        {
            MergePrices(engine);
            MergePriceBasedCalcs(engine);
            MergeVolatility(engine);
            MergeVaR(engine);
        }

        private void MergeVaR(REngine engine)
        {
            engine.Evaluate("res <- data.frame(date=as.character(index(prices)), coredata(prices))");

            // TODO: Do the proper VaR calculation which requires the following:
            // Number of stocks
            // Current price
            // Value of portfolio = price * position
            // Holding period - how long held
            // Some confidence level - Confidence level (5%)
            this._dataService.MergeVaR();
        }

        private void MergeVolatility(REngine engine)
        {
            // using 250 - average number of trading days in a year
            // calculate volatility - (should be adjusted for corporate actions etc, stock splits, dividends)
            engine.Evaluate("vol <- sd(res,na.rm=TRUE) * sqrt(250) * 100");

            var volatility = engine.Evaluate("vol").AsNumeric()[0];

            // pass in the ticker and volatility
            this._dataService.MergeVolatility();
        }

        /// <summary>
        /// Merge Price based calcs
        /// </summary>
        /// <param name="engine"></param>
        private void MergePriceBasedCalcs(REngine engine)
        {
            // So first of all we need to get our prices and calculate the historical returns
            engine.Evaluate("a <- c(prices)");
            engine.Evaluate("res <- ROC(a, type='discrete') * 100");

            this._dataService.MergeHistoricalReturns();
        }
    }
}
