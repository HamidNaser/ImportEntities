using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Importer.Core.Common;

namespace Importers.ImportClient
{
    public interface IBasicEntitiesImporter
    {
        string ClientId { get; set; }
        Task Import();
    }
    
    public interface IEntitiesImporter<T1, T2> : IBasicEntitiesImporter
    {
        HttpClient ClientHttp { get; set; }        
        void SetApiFeedUrls();
    }
}