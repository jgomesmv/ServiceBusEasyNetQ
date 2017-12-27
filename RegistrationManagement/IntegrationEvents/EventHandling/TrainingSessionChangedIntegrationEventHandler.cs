using RegistrationManagement.IntegrationEvents.Events;
using RegistrationManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingManagementSystem.EventBus.Abstractions;

namespace RegistrationManagement.IntegrationEvents.EventHandling
{
    public class TrainingSessionChangedIntegrationEventHandler : IIntegrationEventHandler<TrainingSessionChangedIntegrationEvent>
    {
        public async Task Handle(TrainingSessionChangedIntegrationEvent @event)
        {
            await DataAccess.UpdateSession(new DataAccess.Session { Id = @event.TrainingSessionId, Name = @event.Name, MaxReservationsNumber = @event.MaxReservationsNumber });
        }
    }
}
