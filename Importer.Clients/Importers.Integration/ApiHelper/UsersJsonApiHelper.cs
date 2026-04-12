using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Importer.Core.Common;
using Newtonsoft.Json;
using Serilog;

namespace Importers.Integration.ApiHelper
{
    public class UsersJsonApiHelper<T> : ApiHelper<T> where T : new()
    {
        public UsersJsonApiHelper(IClientInfo clientInfo) : base(clientInfo)
        {
            _clientInfo = clientInfo;
        }

        protected override async Task<List<string>> GetRawDataAsync(string endPoint, CancellationToken ct = default)
        {
            var rawDataList = new List<string>();

            try
            {
                var result = await _clientInfo.ClientHttp.GetStringAsync(endPoint, ct).ConfigureAwait(false);
                rawDataList.Add(result);
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Warning("{@LogMessage}", $"GetRawDataAsync({endPoint}) cancelled.");
                throw;
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
            }

            return rawDataList;
        }

        protected override T GetData(List<string> rawDataList)
        {
            try
            {
                if (rawDataList?.FirstOrDefault() != null)
                {
                    T clientData = JsonConvert.DeserializeObject<T>(rawDataList.FirstOrDefault() ?? string.Empty,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            Error = ImportClientHelper.HandleDeserializationError
                        });

                    SetBlobProperties(clientData, rawDataList, ".json");

                    return clientData;
                }

                return new T();
            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
                return new T();
            }
        }
    }
}
