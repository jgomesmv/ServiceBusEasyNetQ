using EasyNetQ;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography.X509Certificates;
using EasyNetQ.Loggers;

namespace ServiceBusEasyNetQ.Infrastructure.EventBusEasyNetQ
{
    public class DefaultEasyNetQPersisterConnection : IEasyNetQPersisterConnection
    {
        private readonly ILogger<DefaultEasyNetQPersisterConnection> _logger;
        private readonly string _connectionString;
        private IBus _bus;

        bool _disposed;

        public DefaultEasyNetQPersisterConnection(string connectionString,
            ILogger<DefaultEasyNetQPersisterConnection> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _connectionString = connectionString ?? 
                throw new ArgumentNullException(nameof(connectionString));

            _bus = RabbitHutch.CreateBus(_connectionString);
        }

        public string ConnectionString => _connectionString;

        public IBus CreateModel()
        {
            if (!_bus.IsConnected)
            {
                _bus = _bus = RabbitHutch.CreateBus(_connectionString);
            }

            return _bus;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
        }
    }
}
