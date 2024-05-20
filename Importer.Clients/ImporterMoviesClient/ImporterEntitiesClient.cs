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


namespace ImporterMoviesClient
{
    public class MovieImporterClient<T1, T2> : EntitiesImporter<T1, T2>
    {
        public MovieImporterClient(
            IClientInfo clientInfo,
            IApiHelper<T1> apiHelperMovie, 
            IEntitiesService<T2> entitiesService) : base(clientInfo, entitiesService)
        {
            _apiHelpeEntities = apiHelperMovie;
        }
        private async Task AfterRead(List<T2> apiMovies, string apiUrl)
        {
            await Task.Run(() =>
            {
                try
                {
                    apiMovies.ForEach(apiMovie =>
                    {
                        if (apiMovie is not Movie clientProvider)
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
            
            AddEntitiesUrl("https://api.themoviedb.org/3/movie/popular", AfterRead);
        }
    }    
}
