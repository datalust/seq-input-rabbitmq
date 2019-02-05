# Seq.Input.RabbitMQ [![Build status](https://ci.appveyor.com/api/projects/status/lxab9qqdtqupk6y4?svg=true)](https://ci.appveyor.com/project/datalust/seq-input-rabbitmq)

A Seq custom input that pulls events from RabbitMQ. **Requires Seq 5.1+.**

### Getting started

The app is published to NuGet as [_Seq.Input.RabbitMQ_](https://nuget.org/packages/seq.input.rabbitmq). Follow the instructions for [installing a Seq App](https://docs.getseq.net/docs/installing-seq-apps) and start an instance of the app, providing your RabbitMQ server details.

### Sending events to the input

The input accepts events in [compact JSON format](https://github.com/serilog/serilog-formatting-compact#format-details), encoded as UTF-8 text.

The [_Serilog.Sinks.RabbitMQ_ sink](https://github.com/sonicjolt/serilog-sinks-rabbitmq), along with the [_Serilog.Formatting.Compact_ formatter](https://github.com/serilog/serilog-formatting-compact), can be used for this.

See the _Demo_ project included in the repository for an example of client configuration that works with the default input configuration.
