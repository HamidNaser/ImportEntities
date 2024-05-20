using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Serilog;

namespace Importer.Core.Common
{
    public class ImportClientHelper
    {
        public static void HandleDeserializationError(object sender,
            Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }

        public object GetPropertyValue(object source, string propertyName)
        {
            if (source != null)
            {
                var propertyInfo = source.GetType().GetProperty(propertyName);
                if (propertyInfo == null)
                {
                    propertyInfo = source.GetType().GetProperty(propertyName.ToLower());
                }

                return propertyInfo.GetValue(source, null);
            }

            return null;
        }

        public async Task SaveBlobToStorage(Dictionary<string,List<BlobInformation>> blobsMap, string fileName)
        {
            try
            {
                var blobContentType = "application/json";
                
                var blobDetailsMap = new BlobDetailsMap
                {
                    BlobMap  = blobsMap
                };
                
                var serializedBlobDetails = SerializeJson(blobDetailsMap);

                await SaveBlobToAzureBlobStorage(fileName, $"{blobContentType}", serializedBlobDetails);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }
        }
        
        public async Task<BlobDetailsMap> ReadBlobFromStorage(string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var serializedBlob = ReadBlobFromAzureBlobStorage($"prod-{fileName}");
                    
                    if (string.IsNullOrEmpty(serializedBlob))
                    {
                        serializedBlob = ReadBlobFromAzureBlobStorage($"staging-{fileName}");                        
                    }
                
                    var blobDetails = DeserializeJson<BlobDetailsMap>(serializedBlob);

                    return blobDetails;
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, e.ToString());
                }

                return new BlobDetailsMap();
            });
        }


        public T DeserializeJson<T>(string toDeserialize)
        {
            return JsonConvert.DeserializeObject<T>(toDeserialize);
        }

        public string SerializeJson<T>(T toSerialize)
        {
            return JsonConvert.SerializeObject(toSerialize);
        }

        public async Task SaveBlobToAzureBlobStorage(string fileName, string blobContentType, string blobDetails)
        {
            try
            {
                var blobStorageConnectionString =
                    Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING");
                
                var storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("importer-archive");
                
                var blob = container.GetBlockBlobReference($"{fileName}");

                blob.Properties.ContentType = $"{blobContentType}";

                await blob.UploadTextAsync($"{blobDetails}").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }
        }

        public string ReadBlobFromAzureBlobStorage(string fileName)
        {
            try
            {
                var blobStorageConnectionString =
                    Environment.GetEnvironmentVariable("BLOB_STORAGE_CONNECTION_STRING");
                
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);
                CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = serviceClient.GetContainerReference("importer-archive");
                CloudBlockBlob blob = container.GetBlockBlobReference($"{fileName}");

                var blobContent = blob.DownloadTextAsync().Result;

                return blobContent;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }

            return string.Empty;
        }        
    }
}