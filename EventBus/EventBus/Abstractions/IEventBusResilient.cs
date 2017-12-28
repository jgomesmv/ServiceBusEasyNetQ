using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TrainingManagementSystem.EventBus.Events;

namespace TrainingManagementSystem.EventBus.Abstractions
{
    public interface IEventBusResilient
    {
        void Publish(IntegrationEvent @event, Action<Task, IntegrationEvent> ackCallBack);

        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;
    }
}