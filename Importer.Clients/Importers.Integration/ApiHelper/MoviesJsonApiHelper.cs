using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Importer.Core.Common;
using Newtonsoft.Json;
using Serilog;

namespace Importers.Integration.ApiHelper
{
    public class MoviesJsonApiHelper<T> : ApiHelper<T> where T : new()
    {
        public MoviesJsonApiHelper(IClientInfo clientInfo) : base(clientInfo)
        {
            _clientInfo = clientInfo;           
        }

        protected override List<string> GetRawData(string endPoint)
        {
            var rawDataList = new List<string>();
            
            try
            {
                var apiKey = "fb65378988bd7263c04991e50189844f";

                var url = endPoint + $"?api_key={apiKey}&language=en-US&page=1";

                using (WebClient wc = new HttpClientUtils.WebClientWithTimeout())
                {
                    wc.Proxy = null;
                    var result = wc.DownloadString(url);
                    rawDataList.Add(result);
                }

            }
            catch (Exception e)
            {
                Log.Logger.Error("{@LogMessage}", e.GetaAllMessages());
                return rawDataList;
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
