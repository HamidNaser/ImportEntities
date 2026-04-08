using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Importers.ImportClient;
using Importers.Integration.Interfaces;
using Importers.Models.ApiModels;
using Serilog;


namespace ImporterMoviesClient
{
    public class MovieImporterClient<T1, T2> : EntitiesImporter<T1, T2>
    {
        private readonly MovieApiOptions _movieApiOptions;

        public MovieImporterClient(
            IClientInfo clientInfo,
            MovieApiOptions movieApiOptions,
            IApiHelper<T1> apiHelperMovie, 
            IEntitiesService<T2> entitiesService) : base(clientInfo, entitiesService)
        {
            _movieApiOptions = movieApiOptions;
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
            
            AddEntitiesUrl(_movieApiOptions.Endpoint, AfterRead);
        }
    }    
}
