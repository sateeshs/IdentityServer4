using IdentityServer4.Configuration;
using IdentityServer4.MongoDB.DbContexts;
using IdentityServer4.MongoDB.Entities;
using IdentityServer4.MongoDB.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServer4.MongoDB.DbContexts
{
    public class PersistedDeviceFlowDbContext : MongoDBContextBase, IPersistedDeviceFlowDbContext
    {
        private readonly IMongoCollection<DeviceFlowCodes> _persistedDeviceFlowCodes;

        public IQueryable<DeviceFlowCodes> DeviceFlowCodes => _persistedDeviceFlowCodes.AsQueryable();

        public PersistedDeviceFlowDbContext(IOptions<MongoDBConfiguration> settings) : base(settings)
        {
            _persistedDeviceFlowCodes = Database.GetCollection<DeviceFlowCodes>(Constants.TableNames.PersistedGrant);
            CreateIndexes();
        }
        public void CreateIndexes()
        {

            var indexOptions = new CreateIndexOptions() { Background = true };
            var builder = Builders<DeviceFlowCodes>.IndexKeys;

            var keyIndexModel = new CreateIndexModel<DeviceFlowCodes>(builder.Ascending(_ => _.SessionId), indexOptions);
            var subIndexModel = new CreateIndexModel<DeviceFlowCodes>(builder.Ascending(_ => _.SubjectId), indexOptions);
            var clientIdSubIndexModel = new CreateIndexModel<DeviceFlowCodes>(
              builder.Combine(
                  builder.Ascending(_ => _.ClientId),
                  builder.Ascending(_ => _.SubjectId)),
              indexOptions);

            var clientIdSubTypeIndexModel = new CreateIndexModel<DeviceFlowCodes>(
              builder.Combine(
                  builder.Ascending(_ => _.ClientId),
                  builder.Ascending(_ => _.SubjectId),
                  builder.Ascending(_ => _.DeviceCode)),
              indexOptions);

            _persistedDeviceFlowCodes.Indexes.CreateOne(keyIndexModel);
            _persistedDeviceFlowCodes.Indexes.CreateOne(subIndexModel);
            _persistedDeviceFlowCodes.Indexes.CreateOne(clientIdSubIndexModel);
            _persistedDeviceFlowCodes.Indexes.CreateOne(clientIdSubTypeIndexModel);

        }

        public void Dispose()
        {
            //TODO
        }

        public Task InsertOrUpdate(Expression<Func<DeviceFlowCodes, bool>> filter, DeviceFlowCodes entity)
        {
            return _persistedDeviceFlowCodes.ReplaceOneAsync(filter, entity, new UpdateOptions() { IsUpsert = true });
        }

        public Task Remove(Expression<Func<DeviceFlowCodes, bool>> filter)
        {
            return _persistedDeviceFlowCodes.DeleteManyAsync(filter);
        }

        public Task RemoveExpired()
        {
            return Remove(x => x.Expiration < DateTime.UtcNow);
        }
    }
}
