using System;
using RDotNet;
using Ria.Model.Events;

namespace Ria.Calculations.Library.Interfaces
{
    public interface ICashFlowBasedCalculations
    {
        void Calculate(REngine engine);
        Action<CalculationNotificationEvent> NotificationEvent { get; set; }
    }
}
