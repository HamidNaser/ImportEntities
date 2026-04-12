using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Importer.Business.Implementations;
using Importer.Core.Common;
using Importer.DatabaseApi.Interfaces;
using Xunit;

namespace ImportEntities.Tests
{
    public class EntitiesServiceTests
    {
        [Fact]
        public async Task ImportEntities_BatchesRequestsInGroupsOfFive()
        {
            var repository = new FakeEntitiesRepository();
            var service = new TestEntitiesService(new TestClientInfo(), repository);
            var entities = Enumerable.Range(1, 11).Select(index => $"entity-{index}").ToList();

            await service.ImportEntities(entities);

            Assert.Equal(3, repository.BatchSizes.Count);
            Assert.Equal(new[] { 5, 5, 1 }, repository.BatchSizes);
        }

        [Fact]
        public async Task ImportEntities_WithNoInput_DoesNotCallRepository()
        {
            var repository = new FakeEntitiesRepository();
            var service = new TestEntitiesService(new TestClientInfo(), repository);

            await service.ImportEntities(new List<string>());

            Assert.Empty(repository.BatchSizes);
        }

        private sealed class TestEntitiesService : EntitiesService<string>
        {
            public TestEntitiesService(IClientInfo clientInfo, IEntitiesRepository entitiesRepository)
                : base(clientInfo, entitiesRepository)
            {
            }

            protected override Task ImportEntities(string apiEntity)
            {
                CurrentEntity = new Entity();
                return base.ImportEntities(apiEntity);
            }
        }

        private sealed class TestClientInfo : IClientInfo
        {
            public string ClientName { get; set; } = "Tests";

            public string ClientId { get; set; } = "tests";

            public HttpClient ClientHttp { get; set; } = new HttpClient();
        }

        private sealed class FakeEntitiesRepository : IEntitiesRepository
        {
            public List<int> BatchSizes { get; } = new List<int>();

            public Task<string> UpdateEntities(string entities, CancellationToken ct = default)
            {
                var document = JsonDocument.Parse(entities);
                BatchSizes.Add(document.RootElement.GetArrayLength());
                return Task.FromResult(entities);
            }
        }
    }
}