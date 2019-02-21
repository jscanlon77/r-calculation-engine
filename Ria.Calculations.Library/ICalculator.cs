using System;

namespace Ria.Calculations.Library
{
    public interface ICalculator
    {
        void Initialize(string[] ticker, string startDate);
        void Calculate();
    }
}
