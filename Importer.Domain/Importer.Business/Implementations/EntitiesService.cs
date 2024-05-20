#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Importer.DatabaseApi.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace Importer.Business.Implementations
{
    public abstract class EntitiesService<T> : IEntitiesService<T>
    {
        protected IClientInfo _clientInfo;
        
        protected IEntitiesRepository _EntitiesRepository;
        protected Entity? CurrentEntity = null;
        protected EntitiesService(
            IClientInfo clientInfo,
            IEntitiesRepository entitiesRepository)
        {
            _clientInfo = clientInfo;
            _EntitiesRepository = entitiesRepository;
        }

        public HttpClient ClientHttp { set; get; }
        public string ClientId { set; get; }

        protected List<Entity> Entities;
        
        public virtual async Task ImportEntities(List<T> allApiEntities)
        {
            
            
            Log.Logger.Information("{@LogMessage}", "Importer.Business.ImportEntities Started");            
            
            if (!allApiEntities.HasAny())
            {
                return;
            }

            
            Entities = new List<Entity>();


            var iCounter = 0;
            var iCount = allApiEntities.Count;

            foreach (var apiEntity in allApiEntities)
            {
                try
                {
                    CurrentEntity = null;

                    if (IsValidRecord(apiEntity))
                    {
                        await ImportEntities(apiEntity).ConfigureAwait(false);

                        Log.Logger.Information("{@LogMessage}", $"Creating Entity # {iCounter} of {iCount}");
                    }
                    else
                    {
                        Log.Logger.Error("{@LogMessage}", "Invalid Record.");

                    }

                    iCounter++;
                }
                catch (Exception e)
                {
                    Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
                }
            }

            try
            {
                if (Entities.HasAny())
                {
                    await UpdateEntitiesInBatchesAsync().ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
            }

            Log.Logger.Information("{@LogMessage}", "Importer.Business.ImportEntities Ended");            
        }
        
        protected virtual async Task UpdateEntitiesInBatchesAsync()
        {
            await Task.Run(async () =>
            {
                Log.Logger.Information("{@LogMessage}", "Importer.Business.UpdateEntitiesInBatchesAsync Started");

                var totalCount = Entities.Count;

                var batchSize = Entities.Count < 5 ? Entities.Count : 5;
                
                for (int i = 0; i < totalCount; i += batchSize)
                {
                    var entitiesBatch = string.Empty;

                    var apiEntityBatch = Entities.Skip(i).Take(batchSize).ToList();

                    try
                    {
                        
                        entitiesBatch = JsonConvert.SerializeObject(apiEntityBatch);

                        await _EntitiesRepository.UpdateEntities(entitiesBatch).ConfigureAwait(false);

                        Log.Logger.Information("{@LogMessage}", $"Update Entities Table # {i} of {totalCount}");
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Error($"Error {e}, Entities Batch: {entitiesBatch} failed", e.ToString(), entitiesBatch);
                    }
                }

                Log.Logger.Information("{@LogMessage}", "Importer.Business.UpdateentitiesBatchInBatchesAsync Ended");
            });
        }

        


        protected virtual async Task ImportEntities(T apiEntity)
        {
            Entities.Add(CurrentEntity);
        }
        
        protected virtual bool IsValidRecord(T apiEntity)
        {
            return true;
        }
    }
}
