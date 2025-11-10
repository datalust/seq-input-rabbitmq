using System.Threading;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", "Demo")
    .WriteTo.RabbitMQ((client, sink) =>
    {
        client.Hostnames.Add("localhost");
        client.Username = "guest";
        client.Password = "guest";
        client.Exchange = "";
        client.RoutingKey = "logs";
        sink.TextFormatter = new CompactJsonFormatter();
    })
    .CreateLogger();

while (true)
{
    Log.Information("Yo, RabbitMQ!");
    Thread.Sleep(1000);
}
