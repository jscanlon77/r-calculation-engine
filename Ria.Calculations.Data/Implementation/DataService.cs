using System.Collections.Generic;
using System.Data.SqlClient;
using Ria.Calculations.Data.Interfaces;
using Ria.Model.Model;

namespace Ria.Calculations.Data.Implementation
{
    public class DataService : IDataService
    {
        public DataService()
        {

        }
        public void MergePrices(IEnumerable<PriceLineItem> pricesList)
        {
            // do the data implementation and use ADO.NET, ORM, NOSQL or whatever
            // allowing us to be testable.
            // We'll be able to swap out the provider with a factory or some such mechanism.
            
        }

        public void MergeCashFlows(IEnumerable<CashFlowBasedItem> cashFlowBasedItems)
        {
            // some implementation for storing cashflows
        }

        public void MergeXirr()
        {
            // some implementation passing in ticker(s) XIRR dictionary..
        }

        public void MergeHistoricalPositions()
        {
            // Again some implementation which needs to be MUCH MUCH smarter than 
            // I've coded it 
        }

        public void MergeHistoricalReturns()
        {
            // smarter implementation for returns for the calcs engine.
        }

        public void MergeVolatility()
        {
            
        }

        public void MergeInvestedCapital()
        {
            
        }

        public void MergeVaR()
        {
            
        }
    }
}
