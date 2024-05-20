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
using Importers.Models.ApiModels.Users;
using Microsoft.Extensions.Logging;
using Serilog;


namespace Importer.Business.Users
{
    public class ClientUsersService<T1> : EntitiesService<T1>
    {
        public ClientUsersService(
            IClientInfo clientInfo,
            IEntitiesRepository entitiesRepository) : base(clientInfo, entitiesRepository)
        {
        }

        protected override async Task ImportEntities(T1 apiEntity)
        {
            var clientUser = apiEntity as User;

            if (clientUser != null)
            {
                var userEntity = new UserEntity();
                userEntity.FirstName = clientUser.first_name;
                userEntity.LastName = clientUser.last_name;

                CurrentEntity = userEntity;
            }

            await base.ImportEntities(apiEntity).ConfigureAwait(false);
        }

        protected override bool IsValidRecord(T1 apiMovie)
        {
            if (apiMovie == null)
            {
                return false;
            }

            if (apiMovie is not User clientUser)
            {
                return false;
            }

            if (string.IsNullOrEmpty(clientUser.first_name))
            {
                return false;
            }

            return true;
        }

    }

}