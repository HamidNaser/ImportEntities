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
    public class MoviesJsonApiHelper<T> : ApiHelper<T> where T : new()
    {
        private readonly MovieApiOptions _movieApiOptions;

        public MoviesJsonApiHelper(IClientInfo clientInfo, MovieApiOptions movieApiOptions) : base(clientInfo)
        {
            _clientInfo = clientInfo;
            _movieApiOptions = movieApiOptions;
        }

        protected override async Task<List<string>> GetRawDataAsync(string endPoint, CancellationToken ct = default)
        {
            var rawDataList = new List<string>();

            try
            {
                if (string.IsNullOrWhiteSpace(_movieApiOptions.ApiKey))
                {
                    throw new InvalidOperationException(
                        "Movies API key is missing. Configure ApiClients:Movies:ApiKey or TMDB_API_KEY before running the importer.");
                }

                var url = endPoint +
                    $"?api_key={_movieApiOptions.ApiKey}&language={_movieApiOptions.Language}&page={_movieApiOptions.Page}";

                var result = await _clientInfo.ClientHttp.GetStringAsync(url, ct).ConfigureAwait(false);
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
