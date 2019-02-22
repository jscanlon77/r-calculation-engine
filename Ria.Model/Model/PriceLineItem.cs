using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ria.Model.Model
{
    public class PriceLineItem
    {
        public DateTime DateTime { get; }
        public string Ticker { get; }
        public double Price { get; }

        public PriceLineItem(DateTime dateTime, string ticker, double price)
        {
            DateTime = dateTime;
            Ticker = ticker;
            Price = price;
        }
    }
}
