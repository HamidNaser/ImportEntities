using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

// Replace the stub dequeue call with the real Confluent.Kafka client:
//   dotnet add package Confluent.Kafka

namespace Importers.ImportClient.Sources
{
    // Reads entities from a Kafka topic.
    //
    // Usage in ConfigureSources():
    //   AddSource(new KafkaEntitiesSource<MyRecord>(
    //       bootstrapServers: "localhost:9092",
    //       topic:            "my-topic",
    //       groupId:          "importer-group",
    //       mapper:           json => JsonConvert.DeserializeObject<MyRecord>(json),
    //       maxMessages:      500));
    public class KafkaEntitiesSource<T> : IEntitiesSource<T>
    {
        private readonly string _bootstrapServers;
        private readonly string _topic;
        private readonly string _groupId;
        private readonly Func<string, T> _mapper;
        private readonly int _maxMessages;

        public KafkaEntitiesSource(
            string bootstrapServers,
            string topic,
            string groupId,
            Func<string, T> mapper,
            int maxMessages = 1000)
        {
            _bootstrapServers = bootstrapServers;
            _topic = topic;
            _groupId = groupId;
            _mapper = mapper;
            _maxMessages = maxMessages;
        }

        public async Task<EntitySourceResult<T>> Read(CancellationToken ct = default)
        {
            var result = new EntitySourceResult<T> { SourceId = $"kafka://{_bootstrapServers}/{_topic}" };

            try
            {
                Log.Logger.Information("{@LogMessage}", $"KafkaEntitiesSource.Read({_topic}) - Started");

                // ------------------------------------------------------------------
                // Replace this block with real Confluent.Kafka consumer code:
                //
                //   var config = new ConsumerConfig
                //   {
                //       BootstrapServers = _bootstrapServers,
                //       GroupId          = _groupId,
                //       AutoOffsetReset  = AutoOffsetReset.Earliest,
                //   };
                //   using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                //   consumer.Subscribe(_topic);
                //
                //   for (int i = 0; i < _maxMessages; i++)
                //   {
                //       var cr = consumer.Consume(TimeSpan.FromSeconds(2));
                //       if (cr == null) break;
                //       var entity = _mapper(cr.Message.Value);
                //       if (entity != null) result.Entities.Add(entity);
                //   }
                //   consumer.Close();
                // ------------------------------------------------------------------

                await Task.CompletedTask; // remove once real consumer is wired up

                Log.Logger.Information("{@LogMessage}", $"KafkaEntitiesSource.Read({_topic}) - {result.Entities.Count} records");
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", $"KafkaEntitiesSource.Read({_topic}) cancelled.");
                throw;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }

            return result;
        }
    }
}
