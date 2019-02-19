using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ria.Calculations.Library.Model
{
    public class CashFlowBasedItem
    {
        public string Ticker { get; set; }
        public double Xirr { get;set; }
        public DateTime Date { get; set; }
        public double Cashflow { get; set; }
    }
}
