using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ria.Calculations.Library.Events
{
    public class CalculationNotificationEvent
    {
        private readonly string _message;
        private readonly Guid _eventId;

        public CalculationNotificationEvent(string message, Guid eventId)
        {
            _message = message;
            _eventId = eventId;
        }
    }
}
