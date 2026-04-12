using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

// Replace the stub dequeue call with the real AWS SDK:
//   dotnet add package AWSSDK.SQS

namespace Importers.ImportClient.Sources
{
    // Reads entities from an AWS SQS queue.
    //
    // Usage in ConfigureSources():
    //   AddSource(new SqsEntitiesSource<MyRecord>(
    //       queueUrl: "https://sqs.us-east-1.amazonaws.com/123456789/my-queue",
    //       mapper:   json => JsonConvert.DeserializeObject<MyRecord>(json),
    //       maxMessages: 200));
    public class SqsEntitiesSource<T> : IEntitiesSource<T>
    {
        private readonly string _queueUrl;
        private readonly Func<string, T> _mapper;
        private readonly int _maxMessages;

        public SqsEntitiesSource(
            string queueUrl,
            Func<string, T> mapper,
            int maxMessages = 1000)
        {
            _queueUrl = queueUrl;
            _mapper = mapper;
            _maxMessages = maxMessages;
        }

        public async Task<EntitySourceResult<T>> Read(CancellationToken ct = default)
        {
            var result = new EntitySourceResult<T> { SourceId = _queueUrl };

            try
            {
                Log.Logger.Information("{@LogMessage}", $"SqsEntitiesSource.Read({_queueUrl}) - Started");

                // ------------------------------------------------------------------
                // Replace this block with real Amazon.SQS client code:
                //
                //   var sqsClient = new AmazonSQSClient();
                //   int received = 0;
                //   while (received < _maxMessages)
                //   {
                //       var receiveRequest = new ReceiveMessageRequest
                //       {
                //           QueueUrl            = _queueUrl,
                //           MaxNumberOfMessages = Math.Min(10, _maxMessages - received),
                //           WaitTimeSeconds     = 5,
                //       };
                //       var response = await sqsClient.ReceiveMessageAsync(receiveRequest);
                //       if (response.Messages.Count == 0) break;
                //
                //       foreach (var message in response.Messages)
                //       {
                //           var entity = _mapper(message.Body);
                //           if (entity != null) result.Entities.Add(entity);
                //           await sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle);
                //       }
                //       received += response.Messages.Count;
                //   }
                // ------------------------------------------------------------------

                await Task.CompletedTask; // remove once real client is wired up

                Log.Logger.Information("{@LogMessage}", $"SqsEntitiesSource.Read({_queueUrl}) - {result.Entities.Count} records");
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", $"SqsEntitiesSource.Read({_queueUrl}) cancelled.");
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
