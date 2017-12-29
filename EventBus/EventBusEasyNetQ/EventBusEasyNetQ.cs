using Autofac;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using TrainingManagementSystem.EventBus;
using TrainingManagementSystem.EventBus.Abstractions;
using TrainingManagementSystem.EventBus.Events;

namespace TrainingManagementSystem.EventBusEasyNetQ
{
    public class EventBusEasyNetQ : IEventBusResilient
    {
        private readonly IEasyNetQPersisterConnection _serviceBusPersisterConnection;
        private readonly ILogger<DefaultEasyNetQPersisterConnection> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IBus _bus;
        private readonly ILifetimeScope _autofac;
        private readonly string AUTOFAC_SCOPE_NAME = "training_event_bus";
        private const string INTEGRATION_EVENT_SUFIX = "IntegrationEvent";

        public EventBusEasyNetQ(IEasyNetQPersisterConnection serviceBusPersisterConnection,
            ILogger<DefaultEasyNetQPersisterConnection> logger, IEventBusSubscriptionsManager subsManager,
            ILifetimeScope autofac)
        {
            _serviceBusPersisterConnection = serviceBusPersisterConnection;
            _logger = logger;
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _bus = serviceBusPersisterConnection.CreateModel();

            _autofac = autofac;
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);

            var properties = new MessageProperties();
            properties.CorrelationId = eventName;
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _bus.Advanced.PublishAsync(Exchange.GetDefault(), eventName, false, properties, body);
        }

        public void Publish(IntegrationEvent @event, Action<Task, IntegrationEvent> ackCallBack)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFIX, "");
            var jsonMessage = JsonConvert.SerializeObject(@event);

            var properties = new MessageProperties();
            properties.CorrelationId = eventName;
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _bus.Advanced.PublishAsync(Exchange.GetDefault(), eventName, true, properties, body)
                .ContinueWith(response =>
                {
                    ackCallBack(response, @event);
                });
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
                    var queue = _bus.Advanced.QueueDeclare(eventName);
                    var exchange = _bus.Advanced.ExchangeDeclare(eventName, ExchangeType.Fanout);
                    _bus.Advanced.Bind(exchange, queue, eventName);

                    _bus.Advanced.Consume(queue, (body, properties, info) => Task.Factory.StartNew(async () =>
                    {
                        var message = Encoding.UTF8.GetString(body);
                        eventName = $"{properties.CorrelationId}{INTEGRATION_EVENT_SUFIX}";
                        await ProcessEvent(eventName, message);
                    }));
                }
                catch (EasyNetQException)
                {
                    _logger.LogInformation($"The messaging entity {eventName} already exists.");
                }
            }

            _subsManager.AddSubscription<T, TH>();
        }

        public void helper()
        {
            Console.WriteLine($"Event received!");
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFIX, "");

            try
            {
                var queue = _bus.Advanced.QueueDeclare(eventName);
                _bus.Advanced.Consume(queue, msg => Console.WriteLine($"Event unsubscribed: {eventName}"))
                    .Dispose();
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

        private async Task ProcessEvent(string eventName, string message)
        {
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
