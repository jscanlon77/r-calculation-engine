using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
            SetupPath("R-3.4.4");
            REngine.SetEnvironmentVariables();
            REngine engine = REngine.GetInstance();

            this._engine = engine;

            engine.Evaluate("con = gzcon(url('http://www.systematicportfolio.com/sit.gz', 'rb'))");
            engine.Evaluate("source(con)");
            engine.Evaluate("close(con)");
            engine.Evaluate("load.packages('quantmod')");
            engine.Evaluate("load.packages('dplyr)");
            engine.Evaluate("load.packages('PerformanceAnalytics')");
            engine.Evaluate("load.packages('TTR')");
            _engine.Evaluate(@"source('RLibs/xirr/xirrcalc.r')");
            _engine.Evaluate("load.packages('reshape2')");

        }

        private  void SetupPath(string Rversion = "R-3.0.0" ){
            var oldPath = System.Environment.GetEnvironmentVariable("PATH");
            var rPath = System.Environment.Is64BitProcess ? 
                string.Format(@"C:\Program Files\R\{0}\bin\x64", Rversion) :
                string.Format(@"C:\Program Files\R\{0}\bin\i386",Rversion);

            if (!Directory.Exists(rPath))
                throw new DirectoryNotFoundException(
                    string.Format(" R.dll not found in : {0}", rPath));
            var newPath = string.Format("{0}{1}{2}", rPath, 
                System.IO.Path.PathSeparator, oldPath);
            System.Environment.SetEnvironmentVariable("PATH", newPath);
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

        public void Initialize(string[] tickers, string startDate)
        {

            CharacterVector tickerVector = _engine.CreateCharacterVector(tickers);
            CharacterVector dateVector = _engine.CreateCharacterVector(new[] {startDate});

            var appendedTickers = tickers.Select(ticker => $"NASDAQ:{ticker}").ToArray();
            CharacterVector nasdaqCharacterVector = _engine.CreateCharacterVector(appendedTickers);

            _engine.SetSymbol("nasdaqCharacterVector", nasdaqCharacterVector);
            _engine.Evaluate("tickers.temp = spl(nasdaqCharacterVector)");

            _engine.SetSymbol("dateVector", dateVector);

            _engine.SetSymbol("tickerVector", tickerVector);
            _engine.Evaluate("tickers = spl(tickerVector)");

            _engine.Evaluate("data.fund <- new.env()");
            _engine.Evaluate(
                "for (i in 1:len(tickers)) {data.fund[[tickers[i]]] = fund.data(tickers.temp[i], 80, 'annual')};");

            _engine.Evaluate("data <- new.env()");
            _engine.Evaluate("getSymbols(tickers, src = 'yahoo', from = '1970-01-01', env = data, auto.assign = T)");
            _engine.Evaluate("for (i in ls(data)) {data[[i]] = adjustOHLC(data[[i]], use.Adjusted = T)};");


            _engine.Evaluate("portfolioPrices <- NULL");
            _engine.Evaluate("for (Ticker in tickers) {" +
                            $"portfolioPrices <- cbind(portfolioPrices, getSymbols.yahoo(Ticker, from={startDate}, periodicity = \"daily\", auto.assign=FALSE)[,3])" +
                            "}");


        }

        public void Calculate()
        {
            var ibc = _container.Resolve<IInvestmentBasedCalculations>();
            var icc = _container.Resolve<ICashFlowBasedCalculations>();
            var ipb = _container.Resolve<IPriceBasedCalculations>();
            
            ibc.Calculate(_engine);
            ipb.Calculate(_engine);
            icc.Calculate(_engine);
            
        }
    }
}
