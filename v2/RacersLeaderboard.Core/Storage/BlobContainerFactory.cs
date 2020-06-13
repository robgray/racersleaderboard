using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace RacersLeaderboard.Core.Storage
{
    public interface IBlobContainerFactory
    {
        CloudBlobClient GetClient();
        Task<CloudBlobContainer> GetContainer(string containerName, bool createIfNotExists = true);
    }

    public class BlobContainerFactory : IBlobContainerFactory
    {
        private readonly string _connectionString;
        private CloudStorageAccount _cloudStorageAccount;
        private CloudBlobClient _blobClient;
        static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);

        public BlobContainerFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public CloudBlobClient GetClient()
        {
            SemaphoreSlim.Wait();
            try
            {
                if (_blobClient != null) return _blobClient;
                _cloudStorageAccount = CloudStorageAccount.Parse(_connectionString);
                _blobClient = _cloudStorageAccount.CreateCloudBlobClient();
            }
            finally
            {
                SemaphoreSlim.Release();
            }
            return _blobClient;
        }

        public async Task<CloudBlobContainer> GetContainer(string containerName, bool createIfNotExists = true)
        {
            var container = GetClient().GetContainerReference(containerName);
            if (!createIfNotExists) return container;

            await SemaphoreSlim.WaitAsync();
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            finally
            {
                SemaphoreSlim.Release();
            }
            return container;
        }
    }
}
