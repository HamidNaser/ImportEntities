using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Importer.Core.Common;
using Importers.Integration.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace Importers.Integration.ApiHelper
{
    public abstract class ApiHelper<T> : ApiHelperUtil<T>, IApiHelper<T>  where T : new()
    {
        protected IClientInfo _clientInfo;

        protected ApiHelper(IClientInfo clientInfo)
        {
            _clientInfo = clientInfo;
        }

        
        public async Task<Dictionary<string, T>> GetClientDataCollection(List<string> endpoints)
        {
            var clientDataDic = new Dictionary<string, T>();

            return await Task.FromResult(((Func<Dictionary<string, T>>) (() =>
            {
                try
                {
                        endpoints.ForEach(endPoint =>
                        {
                            clientDataDic.Add(endPoint, GetClientData(endPoint).GetAwaiter().GetResult());
                        });
                }
                catch (Exception exception)
                { 
                    Log.Logger.Error("{@LogMessage}", exception.GetaAllMessages());
                }

                return clientDataDic;
            }))());
        }

        public async Task<T> GetClientData(string endpoint)
        {
            return await Task.FromResult(((Func<T>) (() =>
            {
                try
                {
                        var rawData = GetRawData(endpoint);
                        if (rawData.HasAny())
                        {
                            return GetData(rawData);
                        }
                }
                catch (Exception exception)
                {
                    Log.Logger.Error("{@LogMessage}", exception.GetaAllMessages());                
                }

                return default;
            }))());
        }

        protected abstract List<string> GetRawData(string endPoint);

        protected abstract T GetData(List<string> rawData);
    }
}
