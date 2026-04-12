using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Importer.Business.Interfaces
{
    public interface IEntitiesService<T>
    {
        Task ImportEntities(List<T> entities, CancellationToken ct = default);
        HttpClient ClientHttp { set; get; }
        string ClientId { set; get; }
    }
}
