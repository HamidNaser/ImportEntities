using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Importers.ImportClient;
using Importers.Integration.Interfaces;
using Importers.Models.ApiModels;
using Importers.Models.ApiModels.Users;
using Serilog;


namespace ImporterUsersClient
{
    public class UserImporterClient<T1, T2> : EntitiesImporter<T1, T2>
    {
        private readonly UserApiOptions _userApiOptions;
        private readonly IApiHelper<T1> _apiHelperUser;

        public UserImporterClient(
            IClientInfo clientInfo,
            UserApiOptions userApiOptions,
            IApiHelper<T1> apiHelperUser, 
            IEntitiesService<T2> entitiesService) : base(clientInfo, entitiesService)
        {
            _userApiOptions = userApiOptions;
            _apiHelperUser = apiHelperUser;
        }
        private async Task AfterRead(List<T2> apiUsers, string apiUrl)
        {
            await Task.Run(() =>
            {
                try
                {
                    apiUsers.ForEach(apiUser =>
                    {
                        if (apiUser is not User clientUser)
                        {
                            return;
                        }

                    });

                }
                catch (Exception e)
                {
                    Log.Logger.Error(e, e.ToString());
                }
            });
        }
        
        public override async Task Import(CancellationToken ct = default)
        {
            try
            {
                ConfigureSources();
                
                await base.Import(ct).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }
        }
        public override void ConfigureSources()
        {
            base.ConfigureSources();

            AddSource(new ApiHelperEntitiesSource<T1, T2>(
                _userApiOptions.Endpoint,
                _apiHelperUser,
                afterReadBatch: AfterRead));
        }
    }    
}
