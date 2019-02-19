using System;
using CalculationsConsole.Calculations;

namespace CalculationsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new Calculators();
            calculator.Calculate();
           
            Console.ReadLine();
        }
    }
}
