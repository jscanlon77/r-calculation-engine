using Moq;
using NUnit.Framework;
using RDotNet;
using Ria.CalculationEngine.Processors.Interface;
using Ria.Calculations.Data.Interfaces;
using Ria.Calculations.Library.Implementation;
using Ria.Model.Events;
using Ria.Model.Model;

namespace Ria.Calculations.Libraries.Test
{
    [TestFixture]
    public class CashFlowBasedCalculationsTests
    {
        private CashFlowBasedCalculations _cashFlowBasedCalculations;
        private Mock<IDataService> _dataService;
        private Mock<IProducerConsumer<CashFlowBasedItem>> _producerConsumer;

        [SetUp]
        public void BeforeEachTest()
        {
            this._dataService = new Mock<IDataService>();
            this._producerConsumer = new Mock<IProducerConsumer<CashFlowBasedItem>>();
            this._cashFlowBasedCalculations = new CashFlowBasedCalculations(this._dataService.Object, this._producerConsumer.Object);

            // We will need to set up the REngine so that we can test the stuff with info relevant to the 
            // R.DOTNET client and the RLibs.
            // i.e set up the evaluations and the conditions that the engine will test
        }

        [Test]
        public void TestThatWeCanCalculateTheCashFlowStuff()
        {
            
            this._cashFlowBasedCalculations.Calculate(It.IsAny<REngine>());

            var eventWasCalled = false;
            // some assertions that we fired an event and that the data service was properly called with the right data as well.
            // i.e that it was successful.
            this._cashFlowBasedCalculations.NotificationEvent += (notificationEvent) => { eventWasCalled = true; };
            Assert.IsTrue(eventWasCalled);

        }
        
    }
    
}
