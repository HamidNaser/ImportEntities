using System;
using System.Net.Http;
using Newtonsoft.Json;

namespace Importer.Core.Common
{
    public class Entity
    {
    }

    public class UserEntity : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class MovieEntity : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public interface IClientInfo
    {
        public string ClientName { get; set; }
        public string ClientId { get; set; }
        [JsonIgnore]
        public HttpClient ClientHttp { get; set; }

    }
    public class ClientInfoBase : IClientInfo
    {
        public ClientInfoBase()
        {
            ClientHttp = Client;
        }

        public string ClientName { get; set; }
        public string ClientId { get; set; }
        [JsonIgnore]        
        public HttpClient ClientHttp { get; set; }

        
        
        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback =
                HttpClientUtils.GenerateServerCertificateCustomValidationCallback()
        });

    }
}