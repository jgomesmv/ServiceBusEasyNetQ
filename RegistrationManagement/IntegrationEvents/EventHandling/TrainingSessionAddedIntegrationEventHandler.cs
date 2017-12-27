using RegistrationManagement.IntegrationEvents.Events;
using RegistrationManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingManagementSystem.EventBus.Abstractions;

namespace RegistrationManagement.IntegrationEvents.EventHandling
{
    public class TrainingSessionAddedIntegrationEventHandler : IIntegrationEventHandler<TrainingSessionAddedIntegrationEvent>
    {
        public async Task Handle(TrainingSessionAddedIntegrationEvent @event)
        {
            await DataAccess.AddSession(new DataAccess.Session { Id = @event.TrainingSessionId, Name = @event.Name, MaxReservationsNumber = @event.MaxReservationsNumber });
        }
    }
}
