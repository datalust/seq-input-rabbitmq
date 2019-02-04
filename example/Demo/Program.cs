using System;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;

namespace Demo
{
    public class Program
    {
        public static void Main()
        {
            var rmq = new RabbitMQConfiguration
            {
                Hostname = "localhost"
            };

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("Application", "Demo")
                .WriteTo.RabbitMQ(rmq, new CompactJsonFormatter())
                .CreateLogger();

            while (true)
            {
                Log.Information("Yo, RabbitMQ!");
            }
        }
    }
}
