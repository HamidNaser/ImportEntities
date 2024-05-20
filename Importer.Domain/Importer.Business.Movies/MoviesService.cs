#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Importer.Business.Implementations;
using Importer.Business.Interfaces;
using Importer.Core.Common;
using Importer.DatabaseApi.Interfaces;
using Importers.Models.ApiModels;
using Microsoft.Extensions.Logging;
using Serilog;


namespace Importer.Business.Movies
{
    public class ClientMoviesService<T1> : EntitiesService<T1>
    {
        public ClientMoviesService(
            IClientInfo clientInfo,
            IEntitiesRepository entitiesRepository) : base(clientInfo, entitiesRepository)
        {
        }

        protected override async Task ImportEntities(T1 apiEntity)
        {
            var clientMovie = apiEntity as Movie;

            if (clientMovie != null)
            {
                var movieEntity = new MovieEntity();
                movieEntity.Title = clientMovie.title;
                movieEntity.Description = clientMovie.overview;

                CurrentEntity = movieEntity;
            }

            await base.ImportEntities(apiEntity).ConfigureAwait(false);
        }

        protected override bool IsValidRecord(T1 apiEntity)
        {
            if (apiEntity == null)
            {
                return false;
            }

            if (apiEntity is not Movie clientMovie)
            {
                return false;
            }

            if (string.IsNullOrEmpty(clientMovie.title))
            {
                return false;
            }

            return true;
        }

    }

}