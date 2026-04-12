using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Importer.Core.Common;
using Importers.Integration.Interfaces;
using Serilog;

namespace Importers.Integration.ApiHelper
{
    public abstract class ApiHelper<T> : ApiHelperUtil<T>, IApiHelper<T> where T : new()
    {
        protected IClientInfo _clientInfo;

        protected ApiHelper(IClientInfo clientInfo)
        {
            _clientInfo = clientInfo;
        }

        public async Task<Dictionary<string, T>> GetClientDataCollection(List<string> endpoints, CancellationToken ct = default)
        {
            var clientDataDic = new Dictionary<string, T>();

            if (!endpoints.HasAny())
            {
                return clientDataDic;
            }

            try
            {
                foreach (var endPoint in endpoints)
                {
                    ct.ThrowIfCancellationRequested();
                    var clientData = await GetClientData(endPoint, ct).ConfigureAwait(false);
                    clientDataDic[endPoint] = clientData;
                }
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", "GetClientDataCollection cancelled.");
                throw;
            }
            catch (Exception exception)
            {
                Log.Logger.Error("{@LogMessage}", exception.GetaAllMessages());
            }

            return clientDataDic;
        }

        public async Task<T> GetClientData(string endpoint, CancellationToken ct = default)
        {
            try
            {
                var rawData = await GetRawDataAsync(endpoint, ct).ConfigureAwait(false);
                if (rawData.HasAny())
                {
                    return GetData(rawData);
                }
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", $"GetClientData({endpoint}) cancelled.");
                throw;
            }
            catch (Exception exception)
            {
                Log.Logger.Error("{@LogMessage}", exception.GetaAllMessages());
            }

            return default;
        }

        protected abstract Task<List<string>> GetRawDataAsync(string endPoint, CancellationToken ct = default);

        protected abstract T GetData(List<string> rawData);
    }
}
