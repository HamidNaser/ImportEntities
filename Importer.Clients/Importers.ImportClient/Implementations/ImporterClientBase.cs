using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using Importer.Core.Common;
using Importer.DatabaseApi.Interfaces;
using Importer.DatabaseApi.Implementations;

namespace Importers.ImportClient
{
    public static class ImporterClientBaseExtensions
    {
        public static IServiceCollection AddSingletonServices(this IServiceCollection serviceCollection)
        {
            return 
                serviceCollection
                    .AddSingleton<IEntitiesRepository, EntitiesRepository>();
        }
    }


    public interface IClientService 
    {
        public IClientInfo ClientInfo { get; set; }        
    } 
    public class ClientService : IClientService
    {
        public IClientInfo ClientInfo { get; set; }
        public ClientService(IClientInfo clientInfo)
        {
            ClientInfo = clientInfo;
        }
    }

    public class ImporterClientBase
    {
        protected IHost _host;
        protected IHostBuilder _hostBuilder;

        protected IConfiguration _configuration;
        protected ServiceProvider _serviceEntities;
        protected IServiceCollection _services;

        protected IClientInfo _clientInfo;

        protected virtual void InitializeCacheUtils(IServiceCollection services)
        {
            IServiceProvider cacheServiceProvider = services.BuildServiceProvider();
        }

        protected virtual void InitializeLog(IConfigurationBuilder configurationBuilder, string logFileName)
        {
            var filepath = _configuration.GetSection("AppConfig").GetSection("FilePath").Value + logFileName;
            var jsonFormatter = new Serilog.Formatting.Json.JsonFormatter(renderMessage: true);
            var InstrumentationKey = Environment.GetEnvironmentVariable("INSTRUMENTATION_KEY") ?? string.Empty;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configurationBuilder.Build())
                .Enrich.FromLogContext()
                .WriteTo.ApplicationInsights(new TelemetryConfiguration { InstrumentationKey = InstrumentationKey }, TelemetryConverter.Traces)
                .WriteTo.Console()
                .CreateLogger()
                .ForContext("ClientId", _clientInfo.ClientId)
                .ForContext("ClientName", _clientInfo.ClientName);
        }

        protected virtual IHost Initialize(Action<HostBuilderContext, IServiceCollection> configureDependencyInjection, string logFileName)
        {
            var configurationBuilder = new ConfigurationBuilder();

            if (_configuration == null)
            {
                _configuration = configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();
            }

            InitCommonDependencies(configureDependencyInjection);

            _hostBuilder.ConfigureServices(configureDependencyInjection);

            var clientService = ActivatorUtilities.CreateInstance<ClientService>(_host.Services);

            _clientInfo = clientService.ClientInfo;

            InitializeLog(configurationBuilder, logFileName);

            return _host;
        }

        private IHost InitCommonDependencies(Action<HostBuilderContext, IServiceCollection> configureDependencyInjection)
        {
            try
            {

                _hostBuilder = Host.CreateDefaultBuilder();
                var host = _hostBuilder
                    .ConfigureServices((_, s) => InitializeCacheUtils(s))
                    .ConfigureServices(configureDependencyInjection)
                    .UseSerilog()
                    .Build();

                _host = host;

                return host;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }   
}
