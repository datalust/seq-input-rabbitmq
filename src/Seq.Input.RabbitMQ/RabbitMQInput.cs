using System;
using System.IO;
using System.Text;
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
            DisplayName = "RabbitMQ vhost",
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
            DisplayName = "Ssl",
            IsOptional = true,
            HelpText = "Whether or not the connection is with ssl. The default is false.")]
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
                        var clef = body.ToString();
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
