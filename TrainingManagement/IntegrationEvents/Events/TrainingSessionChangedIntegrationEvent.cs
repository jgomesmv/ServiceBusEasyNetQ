using TrainingManagementSystem.EventBus.Events;

namespace TrainingManagement.IntegrationEvents.Events
{
    public class TrainingSessionChangedIntegrationEvent : IntegrationEvent
    {
        public int TrainingSessionId { get; private set; }

        public string Name { get; private set; }

        public int MaxReservationsNumber { get; private set; }

        public TrainingSessionChangedIntegrationEvent(int trainingSessionId, string name, int maxReservationsNumber)
        {
            TrainingSessionId = trainingSessionId;
            Name = name;
            MaxReservationsNumber = maxReservationsNumber;
        }
    }
}
