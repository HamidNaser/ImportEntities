using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Importers.Integration.Interfaces;
using Serilog;

using BlobInformation = Importer.Core.Common.BlobInformation;
using ImportClientHelper = Importer.Core.Common.ImportClientHelper;

namespace Importers.ImportClient
{
    public class EntitiesImporter<T1, T2> : IEntitiesImporter<T1, T2>
    {
        protected IApiHelper<T1> _apiHelpeEntities;
        private List<T2> _Entities;
        protected readonly IEntitiesService<T2> _EntitiesService;
        protected List<string> _EntitiesApiUrls;        
        protected Dictionary<string, Func<T2, Task>> _EntitiesApiUrlsRecordAction;        
        protected Dictionary<string, Func<List<T2>, string, Task>> _EntitiesApiUrlsRecordsAction;
        protected IClientInfo _clientInfo;
        
        public EntitiesImporter(
            IClientInfo clientInfo, 
            IEntitiesService<T2> entitiesService)
        {
            _clientInfo = clientInfo;
            _EntitiesService = entitiesService;
            
            ClientId = clientInfo.ClientId;
            ClientHttp = clientInfo.ClientHttp;
        }

        public string ClientId { get; set; }
        public HttpClient ClientHttp { get; set; }

        protected void AddEntitiesUrl(string url)
        {
            if (_EntitiesApiUrls == null)
            {
                _EntitiesApiUrls = new List<string>();
            }

            if (!string.IsNullOrEmpty(url))
            {
                _EntitiesApiUrls.Add(url);
            }
        }
        protected void AddEntitiesUrl(string url, Func<T2, Task> updateApiEntitiesAfterRead = null)
        {
            if (_EntitiesApiUrlsRecordAction == null)
            {
                _EntitiesApiUrlsRecordAction = new Dictionary<string, Func<T2, Task>>();
            }

            if (!string.IsNullOrEmpty(url))
            {
                _EntitiesApiUrlsRecordAction.Add(url, updateApiEntitiesAfterRead);
            }
        }
        protected void AddEntitiesUrl(string url, Func<List<T2>, string, Task> updateApiEntitiesAfterRead = null)
        {
            if (_EntitiesApiUrlsRecordsAction == null)
            {
                _EntitiesApiUrlsRecordsAction = new Dictionary<string, Func<List<T2>, string, Task>>();
            }
            if (!string.IsNullOrEmpty(url))
            {
                _EntitiesApiUrlsRecordsAction.Add(url, updateApiEntitiesAfterRead);
            }
        }

        public virtual async Task Import()
        {
            try
            {
                Log.Logger.Information("{@LogMessage}", "Entities Import Started");

                if (!_Entities.HasAny())
                {
                    _Entities = await GetEntitiesFromClientApi().ConfigureAwait(false);
                }
                
                Log.Logger.Information("{@LogMessage}", $"Entities Count {_Entities.Count}");                

                if (_Entities != null && _Entities.Count > 0)
                {
                    _EntitiesService.ClientId = ClientId;
                    _EntitiesService.ClientHttp = ClientHttp;

                    var importEntitiesMethod = typeof(IEntitiesService<T2>).GetMethod("ImportEntities");
                    if (importEntitiesMethod != null)
                    {
                        var importEntitiesParams = new List<object>
                        {
                            _Entities,
                        };
                        var importEntitiesMethodResult =
                            (Task) importEntitiesMethod.Invoke(_EntitiesService,
                                importEntitiesParams.ToArray());

                        if (importEntitiesMethodResult != null)
                        {
                            await importEntitiesMethodResult.ConfigureAwait(false);
                        }
                    }
                }

                Log.Logger.Information("{@LogMessage}", "Entities Import Ended");
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
            }
        }

        public virtual void SetApiFeedUrls()
        {
            if (_EntitiesApiUrls.HasAny())
            {
                _EntitiesApiUrls.Clear();
            }

            if (_EntitiesApiUrlsRecordAction.HasAny())
            {
                _EntitiesApiUrlsRecordAction.Clear();
            }

            if (_EntitiesApiUrlsRecordsAction.HasAny())
            {
                _EntitiesApiUrlsRecordsAction.Clear();
            }
        }

        protected virtual async Task<List<T2>> GetEntitiesFromClientApi()
        {
            var allEntities = new List<T2>();
            try
            {
                Log.Logger.Information("{@LogMessage}", "GetEntitiesFromClientApi Started");

                var blobInfosDictionary = new Dictionary<string, List<BlobInformation>>();                
                
                var tasks = new List<Task>();

                var importClientHelper = new ImportClientHelper();

                if (_EntitiesApiUrlsRecordAction != null && _EntitiesApiUrlsRecordAction.Count > 0)
                {
                    foreach (var keyValue in _EntitiesApiUrlsRecordAction)
                    {
                        if (!string.IsNullOrEmpty(keyValue.Key))
                        {
                            Log.Logger.Information("{@LogMessage}", $"GetClientData({keyValue.Key}) - Started");

                            var entitiesWrapper = await _apiHelpeEntities.GetClientData(keyValue.Key).ConfigureAwait(false);

                            var blobCollection = GetEntitiesBlob(entitiesWrapper);
                            if (blobCollection.HasAny())
                            {
                                blobInfosDictionary.Add(keyValue.Key, blobCollection);                                
                            }

                            Log.Logger.Information("{@LogMessage}", $"GetClientData({keyValue.Key}) - Ended");

                            var entities = (List<T2>) importClientHelper.GetPropertyValue(entitiesWrapper, "Entities");

                            if (entitiesWrapper != null && entities != null && entities.Any())
                            {
                                if (entities.HasAny() && keyValue.Value != null)
                                {
                                    try
                                    {
                                        tasks.AddRange(entities.Select(y => keyValue.Value(y)));
                                        await Task.WhenAll(tasks); 
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Logger.Error(e, e.ToString());
                                    }
                                }

                                allEntities.AddRange(entities);
                            }
                        }
                    }
                }

                if (_EntitiesApiUrlsRecordsAction != null && _EntitiesApiUrlsRecordsAction.Count > 0)
                {
                    foreach (var keyValue in _EntitiesApiUrlsRecordsAction)
                    {
                        if (!string.IsNullOrEmpty(keyValue.Key))
                        {
                            Log.Logger.Information("{@LogMessage}", $"GetClientData({keyValue.Key}) - Started");

                            var entitiesWrapper = await _apiHelpeEntities.GetClientData(keyValue.Key).ConfigureAwait(false);

                            var blobCollection = GetEntitiesBlob(entitiesWrapper);
                            if (blobCollection.HasAny())
                            {
                                blobInfosDictionary.Add(keyValue.Key, blobCollection);                                
                            }

                            Log.Logger.Information("{@LogMessage}", $"GetClientData({keyValue.Key}) - Ended");

                            var entities = (List<T2>) importClientHelper.GetPropertyValue(entitiesWrapper, "Entities");

                            if (entitiesWrapper != null && entities != null && entities.Any())
                            {
                                if (entities.HasAny() && keyValue.Value != null)
                                {
                                    try
                                    {
                                        tasks.Add(keyValue.Value(entities, keyValue.Key));
                                        await Task.WhenAll(tasks);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Logger.Error(e, e.ToString());
                                    }
                                }

                                allEntities.AddRange(entities);
                            }
                        }
                    }
                }

                if (_EntitiesApiUrls != null && _EntitiesApiUrls.Count > 0)
                {
                    foreach (var entitiesApiUrl in _EntitiesApiUrls)
                    {
                        if (!string.IsNullOrEmpty(entitiesApiUrl))
                        {
                            Log.Logger.Information("{@LogMessage}", $"GetClientData({entitiesApiUrl}) - Started");

                            var entitiesWrapper = await _apiHelpeEntities.GetClientData(entitiesApiUrl)
                                .ConfigureAwait(false);

                            var blobCollection = GetEntitiesBlob(entitiesWrapper);
                            if (blobCollection.HasAny())
                            {
                                blobInfosDictionary.Add(entitiesApiUrl, blobCollection);                                
                            }

                            Log.Logger.Information("{@LogMessage}", $"GetClientData({entitiesApiUrl}) - Ended");

                            if (entitiesWrapper != null)
                            {
                                var entities = (List<T2>) importClientHelper.GetPropertyValue(entitiesWrapper, "Entities");

                                if (entities != null &&
                                    entities.Any())
                                {
                                    allEntities.AddRange(entities);
                                }
                            }
                        }
                    }
                }

                var currentRunningEnvironment = "prod";
                await importClientHelper.SaveBlobToStorage(blobInfosDictionary,
                        $"{currentRunningEnvironment}-{_clientInfo.ClientName}-{Guid.NewGuid().ToString()}-Entities.json")
                    .ConfigureAwait(false);
                blobInfosDictionary.Clear();
                blobInfosDictionary = null;

                Log.Logger.Information("{@LogMessage}", "GetEntitiesFromClientApi Ended.");
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
                throw;
            }

            return allEntities;
        }
        
        protected List<BlobInformation> GetEntitiesBlob(T1 apiWrapper)
        {
            var blobCollectionPropertyInfo =
                apiWrapper.GetType().GetProperties().ToList()
                    .FirstOrDefault(x =>
                        x.Name.ToLower().Equals("BlobCollection".ToLower()));

            if (blobCollectionPropertyInfo != null)
            {
                var blobCollection = blobCollectionPropertyInfo.GetValue(apiWrapper, null);
                if (blobCollection is List<BlobInformation> blobDetailsCollectionValue)
                {
                    return blobDetailsCollectionValue;
                }
            }

            return null;
        }
    }
}