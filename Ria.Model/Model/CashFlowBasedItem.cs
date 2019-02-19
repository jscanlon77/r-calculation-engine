using System;

namespace Ria.Model.Model
{
    public class CashFlowBasedItem
    {
        public string Ticker { get; set; }
        public double Xirr { get;set; }
        public DateTime Date { get; set; }
        public double Cashflow { get; set; }
    }
}
