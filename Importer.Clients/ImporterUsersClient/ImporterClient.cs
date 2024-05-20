#define TroubleShooting
using System;
using Importer.Business.Interfaces;
using Importer.Business.Users;
using Importer.Core.Common;
using Importers.ImportClient;
using Importers.Integration.ApiHelper;
using Importers.Integration.Interfaces;
using Importers.Models.ApiModels.Users;
using Microsoft.Extensions.DependencyInjection;
using Serilog;


[assembly: System.CLSCompliant(false)]
namespace ImporterUsersClient
{
    public class ClientInfo : ClientInfoBase
    {
        public ClientInfo()
        {
            ClientName = "Users";            
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

            var MovieImporterClient = ActivatorUtilities.CreateInstance<UserImporterClient<UsersWrapper, User>>(_host.Services);

            MovieImporterClient.Import().Wait();

        }

        private void InitializeClientDependencies()
        {
            Initialize((context, services) =>
            {
                _serviceEntities =
                    services
                        .AddSingletonServices()

                        .AddSingleton<IClientInfo, ClientInfo>()

                        .AddSingleton<IClientService, ClientService>()
                        
                        .AddSingleton<IEntitiesService<User>, ClientUsersService<User>>()

                        .AddSingleton<IApiHelper<UsersWrapper>, UsersJsonApiHelper<UsersWrapper>>()

                        .BuildServiceProvider();
                
            }, "Users.txt");
        }
    }
}
