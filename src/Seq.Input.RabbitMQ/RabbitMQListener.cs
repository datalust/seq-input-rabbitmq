using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Seq.Input.RabbitMQ;

class RabbitMQListener(IConnection connection, IChannel channel) : IDisposable
{
    public static async Task<RabbitMQListener> CreateAsync(
        Func<ReadOnlyMemory<byte>, Task> receiveAsync,
        string rabbitMQHost,
        string rabbitMQVHost,
        int rabbitMQPort,
        string rabbitMQUser,
        string rabbitMQPassword,
        string rabbitMQQueue,
        bool isSsl,
        bool isQueueDurable,
        bool isQueueAutoDelete,
        bool isQueueExclusive,
        bool isReceiveAutoAck, 
        string dlx)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMQHost,
            VirtualHost = rabbitMQVHost,
            Port = rabbitMQPort,
            UserName = rabbitMQUser,
            Password = rabbitMQPassword,
            Ssl =
            {
                Enabled = isSsl
            }
        };
            
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var arguments = string.IsNullOrWhiteSpace(dlx) 
            ? null 
            : new Dictionary<string, object> { {"x-dead-letter-exchange", dlx} };
            
        await channel.QueueDeclareAsync(
            rabbitMQQueue, 
            durable: isQueueDurable, 
            exclusive: isQueueExclusive,
            autoDelete: isQueueAutoDelete, 
            arguments: arguments);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) => await receiveAsync(ea.Body);
        await channel.BasicConsumeAsync(rabbitMQQueue, autoAck: isReceiveAutoAck, consumer: consumer);

        return new RabbitMQListener(connection, channel);
    }

    public async Task CloseAsync()
    {
        await channel.CloseAsync();
        await connection.CloseAsync();
    }

    public void Dispose()
    {
        channel?.Dispose();
        connection?.Dispose();
    }
}