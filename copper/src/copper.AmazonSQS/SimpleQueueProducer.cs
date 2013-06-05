using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using Amazon.SQS;
using Amazon.SQS.Model;
using copper.Extensions;
using copper.Json;

namespace copper.AmazonSQS
{
    public class SimpleQueueProducer<T> : UsesBackgroundProducer<T> where T : class
    {
        private string _queue;
        public string QueueUrl { get; private set; }
        private AmazonSQSClient _client;
        private Serializer _serializer;

        public SimpleQueueProducer(string key, string secret, string queue) : this(key, secret, queue, new JsonSerializer())
        {
            
        }

        public SimpleQueueProducer(string key, string secret, string queue, Serializer serializer) : this(new AmazonSQSClient(key, secret), queue, serializer)
        {
            
        }

        public SimpleQueueProducer(AmazonSQSClient client, string queue) : this(client, queue, new JsonSerializer())
        {

        }

        public SimpleQueueProducer(AmazonSQSClient client, string queue, Serializer serializer)
        {
            Initialize(client, queue, serializer);

            Background.Produce(new Func<IEnumerable<T>>(YieldFromQueue)
                                 .AsContinuousObservable()
                                 .Where(e => e != null)
                                 .Buffer(TimeSpan.FromSeconds(1), 10));
        }

        private void Initialize(AmazonSQSClient client, string queue, Serializer serializer)
        {
            _queue = new string(queue.Take(80).ToArray());
            _client = client;
            _serializer = serializer;
            QueueUrl = GetQueueUrl();
        }
        
        private IEnumerable<T> YieldFromQueue()
        {
            var request = new ReceiveMessageRequest()
                .WithMaxNumberOfMessages(10)
                .WithQueueUrl(QueueUrl);

            ReceiveMessageResponse response = null;
            try
            {
                response = _client.ReceiveMessage(request);
            }
            catch
            {
               
            }
            if(response == null || !response.IsSetReceiveMessageResult())
            {
                yield break;
            }

            var messages = response.ReceiveMessageResult.Message;
            var toDelete = new List<DeleteMessageBatchRequestEntry>();
            foreach (var message in messages)
            {
                T deserialized = null;
                try
                {
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(message.Body));
                    deserialized = _serializer.DeserializeFromStream<T>(stream);
                    toDelete.Add(new DeleteMessageBatchRequestEntry()
                        .WithId(message.MessageId)
                        .WithReceiptHandle(message.ReceiptHandle)
                        );
                }
                catch
                {
                        
                }
                yield return deserialized;
            }

            if(toDelete.Count == 0)
            {
                yield break;
            }

            var batch = new DeleteMessageBatchRequest()
                .WithEntries(toDelete.ToArray())
                .WithQueueUrl(QueueUrl);

            _client.DeleteMessageBatch(batch);
        }

        private string GetQueueUrl()
        {
            var request = new CreateQueueRequest();
            request.WithQueueName(_queue).WithDefaultVisibilityTimeout(60);

            var response = _client.CreateQueue(request);
            if (!response.IsSetCreateQueueResult()) throw new InvalidOperationException("Cannot open the queue");
            return response.CreateQueueResult.QueueUrl;
        }
    }
}