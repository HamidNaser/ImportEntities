using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Importer.Core.Common;
using Importers.Integration.Interfaces;
using Serilog;

namespace Importers.ImportClient
{
    public class ApiHelperEntitiesSource<T1, T2> : IEntitiesSource<T2>
    {
        private readonly string _endpoint;
        private readonly IApiHelper<T1> _apiHelper;
        private readonly Func<List<T2>, string, Task> _afterReadBatch;
        private readonly Func<T2, Task> _afterReadRecord;

        public ApiHelperEntitiesSource(
            string endpoint,
            IApiHelper<T1> apiHelper,
            Func<List<T2>, string, Task> afterReadBatch = null,
            Func<T2, Task> afterReadRecord = null)
        {
            _endpoint = endpoint;
            _apiHelper = apiHelper;
            _afterReadBatch = afterReadBatch;
            _afterReadRecord = afterReadRecord;
        }

        public async Task<EntitySourceResult<T2>> Read(CancellationToken ct = default)
        {
            var sourceResult = new EntitySourceResult<T2>
            {
                SourceId = _endpoint,
            };

            if (string.IsNullOrWhiteSpace(_endpoint) || _apiHelper == null)
            {
                return sourceResult;
            }

            try
            {
                Log.Logger.Information("{@LogMessage}", $"GetClientData({_endpoint}) - Started");

                var entitiesWrapper = await _apiHelper.GetClientData(_endpoint, ct).ConfigureAwait(false);
                if (entitiesWrapper == null)
                {
                    return sourceResult;
                }

                var importClientHelper = new ImportClientHelper();
                var entities = importClientHelper.GetPropertyValue(entitiesWrapper, "Entities") as List<T2>;
                var blobCollection = importClientHelper.GetPropertyValue(entitiesWrapper, "BlobCollection") as List<BlobInformation>;

                if (blobCollection.HasAny())
                {
                    sourceResult.BlobCollection = blobCollection;
                }

                if (!entities.HasAny())
                {
                    return sourceResult;
                }

                if (_afterReadRecord != null)
                {
                    var recordTasks = entities.Select(entity => _afterReadRecord(entity));
                    await Task.WhenAll(recordTasks).ConfigureAwait(false);
                }

                if (_afterReadBatch != null)
                {
                    await _afterReadBatch(entities, _endpoint).ConfigureAwait(false);
                }

                sourceResult.Entities = entities;

                Log.Logger.Information("{@LogMessage}", $"GetClientData({_endpoint}) - Ended");
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", $"ApiHelperEntitiesSource.Read({_endpoint}) cancelled.");
                throw;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }

            return sourceResult;
        }
    }
}
