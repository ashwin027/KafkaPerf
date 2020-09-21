using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyKafkaConsumer;
using Shared;

namespace MyKafkaConsumerTasked
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ConsumerConfig _config;

        public Worker(ILogger<Worker> logger, ConsumerConfig config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _config.GroupId = Constants.GroupId;
            using var c = new ConsumerBuilder<Ignore, AnimalInfo>(_config)
                .SetValueDeserializer(new MyDeserializer<AnimalInfo>())
                .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}")).Build();

            c.Subscribe(Constants.Topic);

            try
            {
                var workerBlock = new ActionBlock<ConsumeResult<Ignore, AnimalInfo>>(ProcessMessage, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 6
                });

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = c.Consume(stoppingToken);

                        workerBlock.Post(cr);
                    }
                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Error occurred: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                c.Close();
            }
        }

        private static void ProcessMessage(ConsumeResult<Ignore, AnimalInfo> consumeResult)
        {

            // Adding a 5 millisecond delay to simulate processing
            Thread.Sleep(5);
            Console.WriteLine($"Processed Message with id {consumeResult?.Message?.Value?.MessageId} at: {DateTime.Now:HH:mm:ss.ffff}.");
        }
    }
}
