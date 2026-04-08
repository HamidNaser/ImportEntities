#define TroubleShooting
using System;
using Importer.Business.Interfaces;
using Importer.Business.Movies;
using Importer.Core.Common;
using Importers.ImportClient;
using Importers.Integration.ApiHelper;
using Importers.Integration.Interfaces;
using Importers.Models.ApiModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


[assembly: System.CLSCompliant(false)]
namespace ImporterMoviesClient
{
    public class ClientInfo : ClientInfoBase
    {
        public ClientInfo()
        {
            ClientName = "Movies";            
            ClientId = "9d45ed9f-b278-407b-8fc8-9e0c9cd86826";
        }
    }
    
    public class ImporterClient : ImporterClientBase
    {
        public void Import()
        {
            InitializeClientDependencies();

            if (_clientInfo != null)
            {
                Log.Logger.Information("{@LogMessage}", $"Import {_clientInfo.ClientName} Started");
            }

            var MovieImporterClient = ActivatorUtilities.CreateInstance<MovieImporterClient<MoviesWrapper, Movie>>(_host.Services);

            MovieImporterClient.Import().Wait();

        }

        private void InitializeClientDependencies()
        {
            Initialize((context, services) =>
            {
                var movieApiOptions = _configuration.GetSection("ApiClients:Movies").Get<MovieApiOptions>() ?? new MovieApiOptions();
                movieApiOptions.ApiKey = Environment.GetEnvironmentVariable("TMDB_API_KEY") ?? movieApiOptions.ApiKey;

                var repositoryOptions = _configuration.GetSection("Repository").Get<RepositoryOptions>() ?? new RepositoryOptions();

                _serviceEntities =
                    services
                        .AddSingletonServices()
                        .AddSingleton(movieApiOptions)
                        .AddSingleton(repositoryOptions)

                        .AddSingleton<IClientInfo, ClientInfo>()

                        .AddSingleton<IClientService, ClientService>()
                        
                        .AddSingleton<IEntitiesService<Movie>, ClientMoviesService<Movie>>()

                        .AddSingleton<IApiHelper<MoviesWrapper>, MoviesJsonApiHelper<MoviesWrapper>>()

                        .BuildServiceProvider();
                
            }, "Movies.txt");
        }
    }
}
