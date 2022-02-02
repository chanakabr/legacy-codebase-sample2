using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ
{
    public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private static RabbitMQPersistentConnection _Instance;
        private readonly IConnectionFactory _ConnectionFactory;
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly int _RetryCount;
        private static IConnection _Connection;
        bool _Disposed;

        private static readonly object _SyncRoot = new object();
        private bool _IsConnected => _Connection != null && _Connection.IsOpen && !_Disposed;


        public static RabbitMQPersistentConnection GetInstanceUsingTCMConfiguration()
        {
            var configuration = ApplicationConfiguration.Current.RabbitConfiguration.EventBus;
            if (string.IsNullOrEmpty(configuration.HostName.Value) ||
                configuration.Port.Value <= 0 ||
                string.IsNullOrEmpty(configuration.UserName.Value) ||
                string.IsNullOrEmpty(configuration.Password.Value)
                )
            {
                throw new Exception("rabbit mq configuration is missing for event_bus in tcm");
            }

            if (_Instance == null)
            {
                lock (_SyncRoot)
                {
                    if (_Instance == null)
                    {
                        var connectionFactory = new ConnectionFactory();
                        connectionFactory.HostName = configuration.HostName.Value;
                        connectionFactory.UserName = configuration.UserName.Value;
                        connectionFactory.Password = configuration.Password.Value;
                        connectionFactory.Port = configuration.Port.Value;
                        connectionFactory.DispatchConsumersAsync = true;

                        _Logger.Info($"Constructing connection factory with HostName:[{configuration.HostName.Value}] on port:[{configuration.Port.Value}]");
                        _Instance = new RabbitMQPersistentConnection(connectionFactory, ApplicationConfiguration.Current.QueueFailLimit.Value);
                    }
                }
            }

            return _Instance;
        }

        private RabbitMQPersistentConnection(IConnectionFactory connectionFactory, int retryCount = 5)
        {
            _ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _RetryCount = retryCount;
        }

        public IModel CreateModel()
        {
            _Logger.Info($"Getting RabbitMQ connection");
            if (!_IsConnected)
            {
                _Logger.Info($"Looks like there is not RabbitMQ connection, acquiring lock to connect");
                lock (_SyncRoot)
                {
                    _Logger.Info($"Lock Acquired thread:[{Thread.CurrentThread.ManagedThreadId}],  Getting connection...");
                    if (!_IsConnected)
                    {
                        _Logger.Info($"No connection found to open a channel to RabbitMQ, trying to connect...");
                        TryConnect();
                    }
                }
            }

            return _Connection.CreateModel();
        }

        public void Dispose()
        {
            if (_Disposed) return;

            _Disposed = true;

            try
            {
                _Connection.Dispose();
            }
            catch (IOException ex)
            {
                _Logger.Error(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            _Logger.Info($"RabbitMQ Client is trying to connect.");
            if (_IsConnected)
            {
                _Logger.Info($"Already connected");
                return true;
            }


            var policy = RetryPolicy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _Logger.Warn(ex.ToString());
                }
            );

            policy.Execute(() =>
            {
                _Connection = _ConnectionFactory.CreateConnection("EventBus_Connection");
            });
            if (_IsConnected)
            {
                _Connection.ConnectionShutdown += OnConnectionShutdown;
                _Connection.CallbackException += OnCallbackException;
                _Connection.ConnectionBlocked += OnConnectionBlocked;
                _Logger.Info($"RabbitMQ persistent connection acquired a connection {_Connection.Endpoint.HostName} and is subscribed to failure events");

                return true;
            }
            else
            {
                _Logger.Error("FATAL ERROR: RabbitMQ connections could not be created and opened");

                return false;
            }

        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_Disposed) return;

            _Logger.Warn("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_Disposed) return;

            _Logger.Warn("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_Disposed) return;

            _Logger.Warn("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}
