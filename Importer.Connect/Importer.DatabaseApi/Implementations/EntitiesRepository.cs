using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Importer.Core.Common;
using Importer.DatabaseApi.Interfaces;
using RestSharp;




namespace Importer.DatabaseApi.Implementations
{
    public class EntitiesRepository : IEntitiesRepository
    {

        protected readonly IClientInfo _clientInfo;
        
        public EntitiesRepository(IClientInfo clientInfo)
        {
            _clientInfo = clientInfo;
        }

        public virtual async Task<string> UpdateEntities(string entities)
        {
            var httpClient = new HttpClient();

            var requestString = $"/posts";

            var connectUri = "https://jsonplaceholder.typicode.com";

            var requestUri = new Uri(new Uri(connectUri), requestString);

            var content = new StringContent(entities, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(requestUri, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBuilder = new StringBuilder();
                errorBuilder.AppendLine("Connect entities batch update failed.");
                errorBuilder.AppendLine("Status Code " + response.StatusCode);
                var responseContent = await response.Content.ReadAsStringAsync();
                errorBuilder.AppendLine(responseContent);

            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
