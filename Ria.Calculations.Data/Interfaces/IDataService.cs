using System.Collections.Generic;
using Ria.Model.Model;

namespace Ria.Calculations.Data.Interfaces
{
    public interface IDataService
    {
        void MergePrices(IEnumerable<PriceLineItem> pricesList);
        void MergeCashFlows(IEnumerable<CashFlowBasedItem> cashFlowBasedItems);
        void MergeXirr();
        void MergeHistoricalPositions();
        void MergeHistoricalReturns();
        void MergeVolatility();
        void MergeInvestedCapital();
        void MergeVaR();
    }
}
