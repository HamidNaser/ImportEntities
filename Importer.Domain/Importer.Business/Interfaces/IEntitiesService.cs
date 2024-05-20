using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Importer.Business.Interfaces
{
    public interface IEntitiesService<T>
    {
        public Task ImportEntities(List<T> entities);
        public HttpClient ClientHttp { set; get; }
        public string ClientId { set; get; }
    }
}
