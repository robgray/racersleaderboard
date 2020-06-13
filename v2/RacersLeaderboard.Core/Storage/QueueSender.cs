using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;

namespace RacersLeaderboard.Core.Storage
{
    public interface IQueueSender
    {
        CloudQueueClient GetClient();
        Task<CloudQueue> GetQueueAsync(string queueName, bool createIfNotExists = true);
        Task SendAsync(string queueName, object message);
    }

    public class QueueSender : IQueueSender
    {
        private readonly string _connectionString;
        private CloudQueueClient _client;
        static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public QueueSender(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CloudQueueClient GetClient()
        {
            if (_client != null) return _client;
            var account = CloudStorageAccount.Parse(_connectionString);
            _client = account.CreateCloudQueueClient();
            return _client;
        }

        public async Task<CloudQueue> GetQueueAsync(string queueName, bool createIfNotExists = true)
        {
            SemaphoreSlim.Wait();
            try
            {
                var client = GetClient();
                var queue = client.GetQueueReference(queueName);
                if (createIfNotExists) await queue.CreateIfNotExistsAsync();
                return queue;
            }
            finally
            {
                SemaphoreSlim.Release();
            }
        }

        public async Task SendAsync(string queueName, object message)
        {
            var queue = await GetQueueAsync(queueName);
            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
        }
    }
}
