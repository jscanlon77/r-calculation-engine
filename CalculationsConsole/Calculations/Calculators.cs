
using System;
using Ria.Calculations.Library;

namespace CalculationsConsole.Calculations
{
    public class Calculators
    {
        private Calculator calculator;

        public Calculators()
        {
            this.calculator = new Calculator();

           
        }

        public void Calculate()
        {
            // Pass in a bunch of tickers or just one.. we should probably use multiple options here
            // and allow a full calculation or calculation from T-1 or anything else.
            // our example will use APPLE..
            // TODO: This needs to offer much more options.
            calculator.Initialize(new []{"AAPL","GOOG","MSFT"}, "2016-01-01");

            // TODO - put some error checking here...
            calculator.Calculate();
        }
    }
}
