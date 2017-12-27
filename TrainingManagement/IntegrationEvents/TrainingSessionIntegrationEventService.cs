using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrainingManagementSystem.EventBus.Abstractions;
using TrainingManagementSystem.EventBus.Events;

namespace TrainingManagement.IntegrationEvents
{
    public class TrainingSessionIntegrationEventService : ITrainingSessionIntegrationEventService
    {
        private readonly IEventBus _eventBus;

        public TrainingSessionIntegrationEventService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            _eventBus.Publish(evt);
        }
    }
}
