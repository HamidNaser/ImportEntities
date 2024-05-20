using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Importers.ImportClient;
using Importers.Integration.Interfaces;
using Importers.Models.ApiModels;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.Extensions.Logging;
using Serilog;


namespace ImporterUsersClient
{
    public class UserImporterClient<T1, T2> : EntitiesImporter<T1, T2>
    {
        public UserImporterClient(
            IClientInfo clientInfo,
            IApiHelper<T1> apiHelperUser, 
            IEntitiesService<T2> entitiesService) : base(clientInfo, entitiesService)
        {
            _apiHelpeEntities = apiHelperUser;
        }
        private async Task AfterRead(List<T2> apiUsers, string apiUrl)
        {
            await Task.Run(() =>
            {
                try
                {
                    apiUsers.ForEach(apiUser =>
                    {
                        if (apiUser is not Movie clientUser)
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
        
        public override async Task Import()
        {
            try
            {
                SetApiFeedUrls();
                
                await base.Import().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.ToString());
            }
        }
        public override void SetApiFeedUrls()
        {
            base.SetApiFeedUrls();
            
            AddEntitiesUrl("https://reqres.in/api/users", AfterRead);
        }
    }    
}
