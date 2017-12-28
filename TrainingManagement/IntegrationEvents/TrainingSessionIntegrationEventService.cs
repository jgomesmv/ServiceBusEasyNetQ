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
        private readonly IEventBusResilient _eventBus;

        public TrainingSessionIntegrationEventService(IEventBusResilient eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void AckCallback(Task response, IntegrationEvent @event)
        {
            // this only checks that the task finished
            // IsCompleted will be true even for tasks in a faulted state
            // Task will complete in a faulted state if no confirmation or a NACK confirmation is received
            if (response.IsCompleted && !response.IsFaulted)
            {
                Console.WriteLine("I have done my work!");
            }
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            _eventBus.Publish(evt, AckCallback);
        }
    }
}
