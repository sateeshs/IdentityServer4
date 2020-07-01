using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IdentityServer4.MongoDB.Interfaces;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using IdentityServer4.MongoDB.Entities;
using IdentityModel;
using System.Linq;
using IdentityServer4.Infrastructure;

namespace IdentityServer4.Stores.MongoDB
{
    public class MongoDbDeviceFlowStore : IDeviceFlowStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly IPersistedDeviceFlowDbContext Context;

        /// <summary>
        ///  The serializer.
        /// </summary>
        protected readonly IPersistentGrantSerializer Serializer;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        public MongoDbDeviceFlowStore(IPersistedDeviceFlowDbContext context,
            IPersistentGrantSerializer serializer,
            ILogger<MongoDbDeviceFlowStore> logger)
        {
            Context = context;
            Serializer = serializer;
            Logger = logger;
        }
        public async Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, DeviceCode data)
        {
            var entity = ToEntity(data, deviceCode, userCode);
            await Context.InsertOrUpdate(x => x.ClientId == entity.ClientId, entity);
        }
        public Task<DeviceCode> FindByDeviceCodeAsync(string deviceCode)
        {
            var deviceFlowCodes = Context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == deviceCode);
            var model = ToModel(deviceFlowCodes?.Data);

            Logger.LogDebug("{userCode} found in database: {userCodeFound}", deviceCode, model != null);

            return Task.FromResult(model);
        }

        public Task<DeviceCode> FindByUserCodeAsync(string userCode)
        {
            var deviceFlowCodes = Context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == userCode);
            var model = ToModel(deviceFlowCodes?.Data);

            Logger.LogDebug("{userCode} found in database: {userCodeFound}", userCode, model != null);

            return Task.FromResult(model);
        }

        public Task RemoveByDeviceCodeAsync(string deviceCode)
        {
            Context.Remove(x => x.DeviceCode == deviceCode);
            return Task.FromResult(0);
        }



        public Task UpdateByUserCodeAsync(string userCode, DeviceCode data)
        {
            var entity = ToEntity(data,null,userCode);
             Context.InsertOrUpdate(x => x.ClientId == entity.ClientId, entity);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Converts a model to an entity.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="deviceCode"></param>
        /// <param name="userCode"></param>
        /// <returns></returns>
        protected DeviceFlowCodes ToEntity(DeviceCode model, string deviceCode, string userCode)
        {
            if (model == null || deviceCode == null || userCode == null) return null;

            return new DeviceFlowCodes
            {
                DeviceCode = deviceCode,
                UserCode = userCode,
                ClientId = model.ClientId,
                SubjectId = model.Subject?.FindFirst(JwtClaimTypes.Subject).Value,
                CreationTime = model.CreationTime,
                Expiration = model.CreationTime.AddSeconds(model.Lifetime),
                Data = Serializer.Serialize(model)
            };
        }

        /// <summary>
        /// Converts a serialized DeviceCode to a model.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected DeviceCode ToModel(string entity)
        {
            if (entity == null) return null;

            return Serializer.Deserialize<DeviceCode>(entity);
        }
    }
}
