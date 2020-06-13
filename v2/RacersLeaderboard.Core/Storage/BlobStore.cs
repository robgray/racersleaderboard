using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RacersLeaderboard.Core.Storage
{
    public interface IBlobStore
    {
        Task<List<string>> ListBlobs(string containerName, string folder);
        Task<string> GetBlobString(string containerName, string fileReference);
        Task<CloudBlockBlob> GetBlobReference(string containerName, string fileReference);
        Task<T> GetBlobModel<T>(string containerName, string fileReference) where T : class;
        Task<bool> BlobExists(string containerName, string fileReference);
        Task DeleteBlob(string containerName, string fileReference);
        Task UploadBlobString(string containerName, string fileReference, string content);
        Task UploadBlobModel(string containerName, string fileReference, object content);
        Task<BlobProperties> GetBlobProperties(string containerName, string fileReference);
    }

    public class BlobStore : IBlobStore
    {
        private readonly IBlobContainerFactory _factory;
        private readonly JsonSerializerSettings _serializationSettings;

        public BlobStore(IBlobContainerFactory factory)
        {
            _factory = factory;
            _serializationSettings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task<List<string>> ListBlobs(string containerName, string folder)
        {
            var container = await _factory.GetContainer(containerName);
            BlobContinuationToken token = null;
            var files = new List<string>();
            while (true)
            {
                var result = await container.GetDirectoryReference(folder).ListBlobsSegmentedAsync(token);
                files.AddRange(result.Results.Select(x => x.Uri.ToString()));
                if (result.ContinuationToken == null) break;
                token = result.ContinuationToken;
            }

            return files;
        }

        public async Task<string> GetBlobString(string containerName, string fileReference)
        {
            var container = await _factory.GetContainer(containerName);
            var blob = container.GetBlockBlobReference(fileReference);
            try
            {
                return await blob.DownloadTextAsync();
            }
            catch (StorageException ex)
            {
                if (ex.Message.Contains("not exist"))
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<BlobProperties> GetBlobProperties(string containerName, string fileReference)
        {
            var blob = await GetBlobReference(containerName, fileReference);
            return blob.Properties;
        }

        public async Task<CloudBlockBlob> GetBlobReference(string containerName, string fileReference)
        {
            var container = await _factory.GetContainer(containerName);
            return container.GetBlockBlobReference(fileReference);
        }

        public async Task<bool> BlobExists(string containerName, string fileReference)
        {
            var container = await _factory.GetContainer(containerName);
            var blob = container.GetBlockBlobReference(fileReference);
            return await blob.ExistsAsync();
        }

        public async Task UploadBlobString(string containerName, string fileReference, string content)
        {
            var container = await _factory.GetContainer(containerName);
            var blob = container.GetBlockBlobReference(fileReference);
            await blob.UploadTextAsync(content);
        }

        public Task UploadBlobModel(string containerName, string fileReference, object content)
        {
            var json = JsonConvert.SerializeObject(content, Formatting.None, _serializationSettings);
            return UploadBlobString(containerName, fileReference, json);
        }

        public async Task DeleteBlob(string containerName, string fileReference)
        {
            var container = await _factory.GetContainer(containerName);
            var blob = container.GetBlockBlobReference(fileReference);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<T> GetBlobModel<T>(string containerName, string fileReference) where T : class
        {
            var blobString = await GetBlobString(containerName, fileReference);
            if (blobString != null) return JsonConvert.DeserializeObject<T>(blobString, _serializationSettings);
            return null;
        }
    }
}
