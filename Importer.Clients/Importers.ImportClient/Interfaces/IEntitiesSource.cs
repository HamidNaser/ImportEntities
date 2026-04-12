using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Importer.Core.Common;

namespace Importers.ImportClient
{
    public class EntitySourceResult<T>
    {
        public string SourceId { get; set; }
        public List<T> Entities { get; set; } = new List<T>();
        public List<BlobInformation> BlobCollection { get; set; } = new List<BlobInformation>();
    }

    public interface IEntitiesSource<T>
    {
        Task<EntitySourceResult<T>> Read(CancellationToken ct = default);
    }
}
