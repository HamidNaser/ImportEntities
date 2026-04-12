using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Serilog;

using BlobInformation = Importer.Core.Common.BlobInformation;
using ImportClientHelper = Importer.Core.Common.ImportClientHelper;

namespace Importers.ImportClient
{
    public class EntitiesImporter<T1, T2> : IEntitiesImporter<T1, T2>
    {
        private List<T2> _entities;
        protected readonly IEntitiesService<T2> _EntitiesService;
        protected readonly List<IEntitiesSource<T2>> _entitySources = new List<IEntitiesSource<T2>>();
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

        protected void AddSource(IEntitiesSource<T2> source)
        {
            if (source == null)
            {
                return;
            }

            _entitySources.Add(source);
        }

        public virtual async Task Import(CancellationToken ct = default)
        {
            try
            {
                Log.Logger.Information("{@LogMessage}", "Entities Import Started");

                if (!_entities.HasAny())
                {
                    _entities = await GetEntitiesFromSources(ct).ConfigureAwait(false);
                }
                
                Log.Logger.Information("{@LogMessage}", $"Entities Count {_entities.Count}");                

                if (_entities != null && _entities.Count > 0)
                {
                    _EntitiesService.ClientId = ClientId;
                    _EntitiesService.ClientHttp = ClientHttp;

                    await _EntitiesService.ImportEntities(_entities, ct).ConfigureAwait(false);
                }

                Log.Logger.Information("{@LogMessage}", "Entities Import Ended");
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", "Entities Import cancelled.");
                throw;
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
            }
        }

        public virtual void ConfigureSources()
        {
            if (_entitySources.HasAny())
            {
                _entitySources.Clear();
            }
        }

        protected virtual async Task<List<T2>> GetEntitiesFromSources(CancellationToken ct = default)
        {
            var allEntities = new List<T2>();
            try
            {
                Log.Logger.Information("{@LogMessage}", "GetEntitiesFromSources Started");

                var blobInfosDictionary = new Dictionary<string, List<BlobInformation>>();                
                
                var importClientHelper = new ImportClientHelper();

                foreach (var source in _entitySources)
                {
                    ct.ThrowIfCancellationRequested();

                    if (source == null)
                    {
                        continue;
                    }

                    var sourceResult = await source.Read(ct).ConfigureAwait(false);
                    if (sourceResult == null)
                    {
                        continue;
                    }

                    if (sourceResult.BlobCollection.HasAny())
                    {
                        var sourceId = sourceResult.SourceId ?? $"source-{Guid.NewGuid()}";
                        blobInfosDictionary[sourceId] = sourceResult.BlobCollection;
                    }

                    if (sourceResult.Entities.HasAny())
                    {
                        allEntities.AddRange(sourceResult.Entities);
                    }
                }

                var currentRunningEnvironment = "prod";
                await importClientHelper.SaveBlobToStorage(blobInfosDictionary,
                        $"{currentRunningEnvironment}-{_clientInfo.ClientName}-{Guid.NewGuid().ToString()}-Entities.json")
                    .ConfigureAwait(false);
                blobInfosDictionary.Clear();
                blobInfosDictionary = null;

                Log.Logger.Information("{@LogMessage}", "GetEntitiesFromSources Ended.");
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", "GetEntitiesFromSources cancelled.");
                throw;
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
                throw;
            }

            return allEntities;
        }
    }
}