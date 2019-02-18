namespace Ria.Calculations.Library
{
    public interface ICalculator
    {
        void Initialize(string[] ticker);
        void Calculate();
    }
}
