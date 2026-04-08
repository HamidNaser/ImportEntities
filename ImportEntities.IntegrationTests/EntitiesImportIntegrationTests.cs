using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Importer.Business.Implementations;
using Importer.Core.Common;
using Importer.DatabaseApi.Implementations;
using Xunit;

namespace ImportEntities.IntegrationTests;

public class EntitiesImportIntegrationTests
{
    [Fact]
    public async Task ImportEntities_UsesConfiguredEndpoint_AndSendsBatchesOfFive()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("ok", Encoding.UTF8, "application/json")
        });

        var clientInfo = new TestClientInfo(new HttpClient(handler));
        var repository = new EntitiesRepository(clientInfo, new RepositoryOptions
        {
            BaseUrl = "https://integration.test",
            UpdateEntitiesPath = "/entities/update"
        });

        var service = new TestEntitiesService(clientInfo, repository);
        var entities = Enumerable.Range(1, 11).Select(index => $"entity-{index}").ToList();

        await service.ImportEntities(entities);

        Assert.Equal(3, handler.Requests.Count);
        Assert.All(handler.Requests, request =>
            Assert.Equal("https://integration.test/entities/update", request.RequestUri!.ToString()));

        var batchSizes = handler.Requests
            .Select(request => JsonDocument.Parse(request.Body).RootElement.GetArrayLength())
            .ToList();

        Assert.Equal(new[] { 5, 5, 1 }, batchSizes);
        Assert.All(handler.Requests, request => Assert.Equal("application/json", request.ContentType));
    }

    [Fact]
    public async Task UpdateEntities_WhenApiReturnsFailure_ReturnsResponseContent()
    {
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("failed-update", Encoding.UTF8, "text/plain")
        });

        var clientInfo = new TestClientInfo(new HttpClient(handler));
        var repository = new EntitiesRepository(clientInfo, new RepositoryOptions
        {
            BaseUrl = "https://integration.test",
            UpdateEntitiesPath = "/entities/update"
        });

        var response = await repository.UpdateEntities("[]");

        Assert.Equal("failed-update", response);
        Assert.Single(handler.Requests);
    }

    private sealed class TestEntitiesService : EntitiesService<string>
    {
        public TestEntitiesService(IClientInfo clientInfo, Importer.DatabaseApi.Interfaces.IEntitiesRepository entitiesRepository)
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
        public TestClientInfo(HttpClient clientHttp)
        {
            ClientHttp = clientHttp;
        }

        public string ClientName { get; set; } = "IntegrationTests";

        public string ClientId { get; set; } = "integration-tests";

        public HttpClient ClientHttp { get; set; }
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public List<CapturedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var contentType = request.Content?.Headers.ContentType?.MediaType ?? string.Empty;

            Requests.Add(new CapturedRequest(request.RequestUri, body, contentType));
            return _responseFactory(request);
        }
    }

    private sealed record CapturedRequest(Uri? RequestUri, string Body, string ContentType);
}
