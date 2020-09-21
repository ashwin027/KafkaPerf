using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyKafkaConsumerTasked
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var brokerList = hostContext.Configuration["KafkaBrokerList"];
                    var consumerConfig = new ConsumerConfig
                    {
                        BootstrapServers = brokerList,
                        AutoOffsetReset = AutoOffsetReset.Earliest,
                        // Group ID is set per consumer
                        //Other consumer settings goes here
                    };
                    services.AddSingleton(consumerConfig);
                    services.AddHostedService<Worker>();
                });
    }
}
