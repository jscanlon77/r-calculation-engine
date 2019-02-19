using System;

namespace Ria.Model.Events
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
