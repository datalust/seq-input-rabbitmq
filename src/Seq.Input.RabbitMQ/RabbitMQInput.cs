using System;
using System.IO;
using System.Text;
using RabbitMQ.Client;
using Seq.Apps;

namespace Seq.Input.RabbitMQ
{
    [SeqApp("RabbitMQ Input",
        Description = "Pulls JSON-formatted events from a RabbitMQ queue. For details of the " +
                      "supported JSON schema, see " +
                      "https://github.com/serilog/serilog-formatting-compact/#format-details.")]
    public class RabbitMQInput : SeqApp, IPublishJson, IDisposable
    {
        RabbitMQListener _listener;

        [SeqAppSetting(
            DisplayName = "RabbitMQ host",
            IsOptional = true,
            HelpText = "The hostname on which RabbitMQ is running. The default is `localhost`.")]
        public string RabbitMQHost { get; set; } = "localhost";

        [SeqAppSetting(
            DisplayName = "RabbitMQ Virtual Host",
            IsOptional = true,
            HelpText = "The virtual host in RabbitMQ. The default is `/`.")]
        public string RabbitMQVHost { get; set; } = "/";

        [SeqAppSetting(
            DisplayName = "RabbitMQ port",
            IsOptional = true,
            HelpText = "The port on which the RabbitMQ server is listening. The default is `5672`.")]
        public int RabbitMQPort { get; set; } = 5672;

        [SeqAppSetting(
            DisplayName = "RabbitMQ user",
            IsOptional = true,
            HelpText = "The username provided when connecting to RabbitMQ. The default is `guest`.")]
        public string RabbitMQUser { get; set; } = "guest";

        [SeqAppSetting(
            DisplayName = "RabbitMQ password",
            IsOptional = true,
            InputType = SettingInputType.Password,
            HelpText = "The password provided when connecting to RabbitMQ. The default is `guest`.")]
        public string RabbitMQPassword { get; set; } = "guest";

        [SeqAppSetting(
            DisplayName = "RabbitMQ queue",
            IsOptional = true,
            HelpText = "The RabbitMQ queue name to receive events from. The default is `Logs`.")]
        public string RabbitMQQueue { get; set; } = "logs";

        [SeqAppSetting(
            DisplayName = "RabbitMQ exchange name",
            IsOptional = true,
            HelpText = "The name of the RabbitMQ exchange from which to pull events. This is the exchange " +
                       "where the messages are published.")]
        public string rabbitMQExchangeName { get; set; } = "";

        [SeqAppSetting(
            DisplayName = "RabbitMQ exchange type",
            IsOptional = true,
            HelpText = "The type of the RabbitMQ exchange (e.g., direct, topic, fanout, or headers). " +
                       "Determines how messages are routed to the queue.")]
        public string rabbitMQExchangeType { get; set; } = ExchangeType.Direct;

        [SeqAppSetting(
            DisplayName = "RabbitMQ Route key",
            IsOptional = true,
            HelpText = "The routing key used for binding the queue to the exchange. " +
                       "This key is used to route messages from the exchange to the queue.")]
        public string rabbitMQRouteKey { get; set; } = "";

        [SeqAppSetting(
            DisplayName = "Require SSL",
            IsOptional = true,
            HelpText = "Whether or not the connection is with SSL. The default is false.")]
        public bool IsSsl { get; set; }

        [SeqAppSetting(
            DisplayName = "Durable",
            IsOptional = true,
            HelpText = "Whether or not the queue is durable. The default is false.")]
        public bool IsQueueDurable { get; set; }

        [SeqAppSetting(
            DisplayName = "Exclusive",
            IsOptional = true,
            HelpText = "Whether or not the queue is exclusive. The default is false.")]
        public bool IsQueueExclusive { get; set; }

        [SeqAppSetting(
            DisplayName = "Auto-delete",
            IsOptional = true,
            HelpText = "Whether or not the queue subscription is durable. The default is false.")]
        public bool IsQueueAutoDelete { get; set; }

        [SeqAppSetting(
            DisplayName = "Auto-ACK",
            IsOptional = true,
            HelpText = "Whether or not messages should be auto-acknowledged. The default is true.")]
        public bool IsReceiveAutoAck { get; set; } = true;

        public void Start(TextWriter inputWriter)
        {
            var sync = new object();

            void Receive(ReadOnlyMemory<byte> body)
            {
                try
                {
                    lock (sync)
                    {
                        var clef = Encoding.UTF8.GetString(body.ToArray());
                        inputWriter.WriteLine(clef);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "A received message could not be decoded");
                }
            }

            _listener = new RabbitMQListener(
                Receive,
                RabbitMQHost,
                RabbitMQVHost,
                RabbitMQPort,
                RabbitMQUser,
                RabbitMQPassword,
                RabbitMQQueue,
                rabbitMQExchangeName,
                rabbitMQExchangeType,
                rabbitMQRouteKey,
                IsSsl,
                IsQueueDurable,
                IsQueueAutoDelete,
                IsQueueExclusive,
                IsReceiveAutoAck);
        }

        public void Stop()
        {
            _listener.Close();
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }
    }
}