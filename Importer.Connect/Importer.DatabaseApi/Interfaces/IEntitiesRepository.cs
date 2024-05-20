using System.Collections.Generic;
using System.Threading.Tasks;
using Importer.Core.Common;

namespace Importer.DatabaseApi.Interfaces
{
    public interface IEntitiesRepository
    {
        Task<string> UpdateEntities(string entities);
    }
}
