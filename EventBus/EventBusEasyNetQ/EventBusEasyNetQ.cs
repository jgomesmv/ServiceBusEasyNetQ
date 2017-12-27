﻿using System.Security.Cryptography.X509Certificates;
using EasyNetQ;
using EasyNetQ.Topology;
using RabbitMQ.Client.Framing.Impl;

namespace ServiceBusEasyNetQ.Infrastructure.EventBusEasyNetQ
{
    using Autofac;
    using EasyNetQ.NonGeneric;
    using ServiceBusMassTransit.Infrastructure.EventBus;
    using EventBus.Abstractions;
    using EventBus.Events;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public class EventBusEasyNetQ : IEventBus
    {
        private readonly IEasyNetQPersisterConnection _serviceBusPersisterConnection;
        private readonly ILogger<EventBusEasyNetQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IBus _bus;
        private readonly ILifetimeScope _autofac;
        private readonly string AUTOFAC_SCOPE_NAME = "training_event_bus";
        private const string INTEGRATION_EVENT_SUFIX = "IntegrationEvent";

        public EventBusEasyNetQ(IEasyNetQPersisterConnection serviceBusPersisterConnection,
            ILogger<EventBusEasyNetQ> logger, IEventBusSubscriptionsManager subsManager, IBus bus,
            ILifetimeScope autofac)
        {
            _serviceBusPersisterConnection = serviceBusPersisterConnection;
            _logger = logger;
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _bus = bus;

            _autofac = autofac;
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);

            var properties = new MessageProperties();
            properties.MessageId = new Guid().ToString();
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _bus.Advanced.Publish(Exchange.GetDefault(), eventName, false, properties, body);
        }

        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.AddDynamicSubscription<TH>(eventName);
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFIX, "");

            var containsKey = _subsManager.HasSubscriptionsForEvent<T>();
            if (!containsKey)
            {
                try
                {
                    _bus.Subscribe<T>(eventName, msg => ProcessEvent<T>(msg));
                }
                catch (EasyNetQException)
                {
                    _logger.LogInformation($"The messaging entity {eventName} already exists.");
                }
            }

            _subsManager.AddSubscription<T, TH>();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFIX, "");

            try
            {
                var subscriptionResult = _bus.Subscribe<T>(eventName, msg => Console.WriteLine($"Event unsubscribed: {eventName}"));
                subscriptionResult.ConsumerCancellation.Dispose();
            }
            catch (EasyNetQException)
            {
                _logger.LogInformation($"The messaging entity {eventName} Could not be found.");
            }

            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _subsManager.Clear();
        }

        private async Task ProcessEvent<T>(T obj)
        {
            string eventName = (string)obj.GetType().GetProperty("routingKey").GetValue(obj, null);
            string message = Encoding.UTF8.GetString((byte[])obj.GetType().GetProperty("Body").GetValue(obj, null));

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _autofac.BeginLifetimeScope(AUTOFAC_SCOPE_NAME))
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            var handler = scope.ResolveOptional(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                            dynamic eventData = JObject.Parse(message);
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                            var handler = scope.ResolveOptional(subscription.HandlerType);
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                            await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                        }
                    }
                }
            }
        }
    }
}