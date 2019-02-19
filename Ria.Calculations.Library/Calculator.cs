using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using RDotNet;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Implementation;
using Ria.Calculations.Library.Interfaces;
using Ria.CalculationEngine.Processors;
using Ria.CalculationEngine.Processors.Implementation;
using Ria.CalculationEngine.Processors.Interface;
using Ria.Calculations.Data.Implementation;

namespace Ria.Calculations.Library
{
    public class Calculator : ICalculator
    {
        public Calculator()
        {
            
            this.ConfigureContainer();
            this.ConfigureLibraries();
        }

        private void ConfigureLibraries()
        {
            REngine.SetEnvironmentVariables();
            REngine engine = REngine.GetInstance();

            this._engine = engine;

            engine.Evaluate("con = gzcon(url('http://www.systematicportfolio.com/sit.gz', 'rb'))");
            engine.Evaluate("source(con)");
            engine.Evaluate("close(con)");
            engine.Evaluate("load.packages('quantmod')");
            engine.Evaluate("load.packages('TTR')");
            //_engine.Evaluate(@"source('libraries/xirr/xirrcalc.r')");

        }

        private void ConfigureContainer()
        {
            _container = new WindsorContainer();
            _container.Register(Component.For<IPriceBasedCalculations>().ImplementedBy<PriceBasedCalculations>());
            _container.Register(Component.For<IInvestmentBasedCalculations>().ImplementedBy<InvestmentBasedCalculations>());
            _container.Register(Component.For<ICashFlowBasedCalculations>().ImplementedBy<CashFlowBasedCalculations>());
            _container.Register(Component.For<IDataService>().ImplementedBy<DataService>());
            _container.Register(AllTypes.FromAssemblyNamed("Ria.CalculationEngine.Processors")
                .BasedOn(typeof(IProducerConsumer<>))
                .WithService.Base());


        }


        private REngine _engine;
        private WindsorContainer _container;
        public void Initialize(string[] tickers)
        {

            CharacterVector tickerVector = _engine.CreateCharacterVector(tickers);

            var appendedTickers = tickers.Select(ticker => $"NASDAQ:{ticker}").ToArray();
            CharacterVector nasdaqCharacterVector = _engine.CreateCharacterVector(appendedTickers);

            _engine.SetSymbol("nasdaqCharacterVector", nasdaqCharacterVector);
            _engine.Evaluate("tickers.temp = spl(nasdaqCharacterVector)");

            _engine.SetSymbol("tickerVector", tickerVector);
            _engine.Evaluate("tickers = spl(tickerVector)"); // print out in the console

            _engine.Evaluate("data.fund <- new.env()");
            _engine.Evaluate("for (i in 1:len(tickers)) {data.fund[[tickers[i]]] = fund.data(tickers.temp[i], 80, 'annual')};");

            // Load some pricing data into a data frame and then get it out in .NET
            _engine.Evaluate("data <- new.env()");
            _engine.Evaluate("getSymbols(tickers, src = 'yahoo', from = '1970-01-01', env = data, auto.assign = T)");
            _engine.Evaluate("for (i in ls(data)) {data[[i]] = adjustOHLC(data[[i]], use.Adjusted = T)};");


            _engine.Evaluate("fund = data.fund[[tickers[1]]]");
            _engine.Evaluate("fund.date = date.fund.data(fund)");

        }

        public void Calculate()
        {
            _container.Resolve<IInvestmentBasedCalculations>().Calculate(_engine);
            _container.Resolve<ICashFlowBasedCalculations>().Calculate(_engine);
            _container.Resolve<IPriceBasedCalculations>().Calculate(_engine);
        }
    }
}
