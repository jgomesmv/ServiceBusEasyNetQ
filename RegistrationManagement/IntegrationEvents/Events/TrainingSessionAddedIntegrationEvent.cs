using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingManagementSystem.EventBus.Events;

namespace RegistrationManagement.IntegrationEvents.Events
{
    public class TrainingSessionAddedIntegrationEvent : IntegrationEvent
    {
        public int TrainingSessionId { get; private set; }

        public string Name { get; private set; }

        public int MaxReservationsNumber { get; private set; }

        public TrainingSessionAddedIntegrationEvent(int trainingSessionId, string name, int maxReservationsNumber)
        {
            TrainingSessionId = trainingSessionId;
            Name = name;
            MaxReservationsNumber = maxReservationsNumber;
        }
    }
}
