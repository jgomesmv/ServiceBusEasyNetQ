namespace ServiceBusEasyNetQ.Infrastructure.EventBusEasyNetQ
{
    using EasyNetQ;
    using System;

    public interface IEasyNetQPersisterConnection : IDisposable
    {
        string ConnectionString { get; }

        IBus CreateModel();
    }
}