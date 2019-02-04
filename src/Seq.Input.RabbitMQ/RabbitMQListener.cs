using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Seq.Input.RabbitMQ
{
    class RabbitMQListener : IDisposable
    {
        readonly IConnection _connection;
        readonly IModel _channel;

        public RabbitMQListener(
            Action<byte[]> receive,
            string rabbitMQHost, 
            int rabbitMQPort, 
            string rabbitMQUser, 
            string rabbitMQPassword,
            string rabbitMQQueue, 
            bool isQueueDurable, 
            bool isQueueAutoDelete, 
            bool isQueueExclusive,
            bool isReceiveAutoAck)
        {
            var factory = new ConnectionFactory {
                HostName = rabbitMQHost,
                Port = rabbitMQPort,
                UserName = rabbitMQUser,
                Password = rabbitMQPassword
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                rabbitMQQueue, 
                durable: isQueueDurable, 
                exclusive: isQueueExclusive,
                autoDelete: isQueueAutoDelete, 
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) => receive(ea.Body);
            _channel.BasicConsume(rabbitMQQueue, autoAck: isReceiveAutoAck, consumer: consumer);
        }

        public void Close()
        {
            _channel.Close();
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Close();
        }
    }
}