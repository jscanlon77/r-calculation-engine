namespace Ria.Calculations.Data.Interfaces
{
    public interface IDataService
    {
        void MergePrices();
        void MergeCashFlows();
        void MergeXirr();
        void MergeHistoricalPositions();
        void MergeHistoricalReturns();
        void MergeVolatility();
        void MergeInvestedCapital();
        void MergeVaR();
    }
}
