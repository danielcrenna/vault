using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.SQS;
using Amazon.SQS.Model;
using copper.Json;

namespace copper.AmazonSQS
{
    /// <summary>
    /// A consumer that enqueues on Amazon SQS. For performance reasons, events handled by this 
    /// consumer are batched on a rolling interval, which is set according to SQS's 10 message maximum,
    /// or every 5 seconds, whichever comes first. If the message payload totals more than 64KB, then
    /// the batch is further broken down into smaller batches.
    /// <seealso href="http://aws.amazon.com/sqs/faqs" />
    /// </summary>
    /// <remarks>
    /// - The message body can contain up to 64 KB of text in any format.
    /// - Messages can be sent, received or deleted in batches of up to 10 messages or 64kb.
    /// - Messages can be retained in queues for up to 14 days.
    /// - Messages can be sent and read simultaneously.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class SimpleQueueConsumer<T> : BatchingConsumer<T> where T : class
    {
        public static int DefaultBatchSize { get; set; }
        public static TimeSpan DefaultBatchInterval { get; set; }

        static SimpleQueueConsumer()
        {
            DefaultBatchSize = 10;
            DefaultBatchInterval = TimeSpan.FromSeconds(5);
        }

        private string _queue;
        public string QueueUrl { get; private set; }
        private AmazonSQSClient _client;
        private Serializer _serializer;
        public ConcurrentQueue<T> Undeliverable { get; private set; }

        public SimpleQueueConsumer(string key, string secret, string queue) : this(new AmazonSQSClient(key, secret), queue, DefaultBatchInterval, new JsonSerializer())
        {

        }

        public SimpleQueueConsumer(string key, string secret, string queue, TimeSpan batchInterval) : this(new AmazonSQSClient(key, secret), queue, batchInterval, new JsonSerializer())
        {

        }

        public SimpleQueueConsumer(string key, string secret, string queue, TimeSpan batchInterval, Serializer serializer) : this(new AmazonSQSClient(key, secret), queue, batchInterval, serializer )
        {
            
        }

        public SimpleQueueConsumer(AmazonSQSClient client, string queue) : this(client, queue, DefaultBatchInterval)
        {
            
        }

        public SimpleQueueConsumer(AmazonSQSClient client, string queue, TimeSpan batchInterval) : this(client, queue, batchInterval, new JsonSerializer())
        {
            
        }

        public SimpleQueueConsumer(AmazonSQSClient client, string queue, TimeSpan batchInterval, Serializer serializer) : base(DefaultBatchSize, batchInterval)
        {
            Initialize(client, queue, serializer);
        }
        
        private void Initialize(AmazonSQSClient client, string queue, Serializer serializer)
        {
            _queue = new string(queue.Take(80).ToArray());
            _client = client;
            _serializer = serializer;
            Undeliverable = new ConcurrentQueue<T>();
            QueueUrl = GetQueueUrl();
        }

        // http://docs.amazonwebservices.com/AWSSimpleQueueService/latest/APIReference/Query_QueryCreateQueue.html
        private string GetQueueUrl()
        {
            var request = new CreateQueueRequest();
            request.WithQueueName(_queue).WithDefaultVisibilityTimeout(60);

            var response = _client.CreateQueue(request);
            if (!response.IsSetCreateQueueResult()) throw new InvalidOperationException("Cannot open the queue");
            return response.CreateQueueResult.QueueUrl;
        }

        public override bool Handle(IList<T> batch)
        {
            if (batch.Count == 0) return true;
            var map = batch.ToDictionary(item => Guid.NewGuid());
            var request = CreateBatchRequest(map);

            var response = _client.SendMessageBatch(request);
            if (response.IsSetSendMessageBatchResult())
            {
                var result = response.SendMessageBatchResult;
                if (result.IsSetBatchResultErrorEntry())
                {
                    return result.BatchResultErrorEntry.Count == 0 || CollectErrors(result, map);
                }
            }
            return false;
        }

        private bool CollectErrors(SendMessageBatchResult result, IDictionary<Guid, T> map)
        {
            var errorIds = new HashSet<Guid>(result.BatchResultErrorEntry.Select(e => new Guid(e.Id)));
            foreach (var errorId in errorIds)
            {
                T @event;
                if (map.TryGetValue(errorId, out @event))
                {
                    Undeliverable.Enqueue(@event);
                }
            }
            return false;
        }
        
        private SendMessageBatchRequest CreateBatchRequest(Dictionary<Guid, T> map)
        {
            var entries = new List<SendMessageBatchRequestEntry>();
            foreach (var item in map)
            {
                var serialized = SerializeEvent(item.Value);
                if (serialized == null) continue;
                var entry = new SendMessageBatchRequestEntry {Id = item.Key.ToString(), MessageBody = serialized};
                entries.Add(entry);
            }
            var request = new SendMessageBatchRequest { QueueUrl = QueueUrl, Entries = entries };
            return request;
        }

        private string SerializeEvent(T @event)
        {
            try
            {
                string serialized;
                using (var stream = _serializer.SerializeToStream(@event))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    serialized = Encoding.UTF8.GetString(new MemoryStream(stream.ReadFully()).ToArray());
                }
                return serialized;
            }
            catch (Exception)
            {
                Undeliverable.Enqueue(@event);
                return null;
            }
        }
    }
}
