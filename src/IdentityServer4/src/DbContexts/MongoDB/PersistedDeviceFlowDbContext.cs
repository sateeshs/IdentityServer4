using IdentityServer4.Configuration;
using IdentityServer4.Infrastructure;
using IdentityServer4.MongoDB.Entities;
using IdentityServer4.MongoDB.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IdentityServer4.MongoDB.DbContexts
{
    public class PersistedDeviceFlowDbContext : MongoDBContextBase, IPersistedDeviceFlowDbContext
    {
        private readonly IMongoCollection<DeviceFlowCodes> _deviceFlowCodes;

        
        public PersistedDeviceFlowDbContext(IOptions<MongoDBConfiguration> settings) :base(settings) {
            _deviceFlowCodes = Database.GetCollection<DeviceFlowCodes>(Constants.TableNames.PersistedGrant);
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexOptions = new CreateIndexOptions() { Background = true };
            var builder = Builders<DeviceFlowCodes>.IndexKeys;

            var keyIndexModel = new CreateIndexModel<DeviceFlowCodes>(builder.Ascending(_ => _.ClientId), indexOptions);
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
                  builder.Ascending(_ => _.SessionId)),
              indexOptions);

            _deviceFlowCodes.Indexes.CreateOne(keyIndexModel);
            _deviceFlowCodes.Indexes.CreateOne(subIndexModel);
            _deviceFlowCodes.Indexes.CreateOne(clientIdSubIndexModel);
            _deviceFlowCodes.Indexes.CreateOne(clientIdSubTypeIndexModel);
        }


        public IQueryable<DeviceFlowCodes> DeviceFlowCodes => _deviceFlowCodes.AsQueryable();

        public Task InsertOrUpdate(Expression<Func<DeviceFlowCodes, bool>> filter, DeviceFlowCodes entity)
        {
            return _deviceFlowCodes.ReplaceOneAsync(filter, entity, new UpdateOptions() {IsUpsert = true});
        }

        public Task Remove(Expression<Func<DeviceFlowCodes, bool>> filter)
        {
            return _deviceFlowCodes.DeleteManyAsync(filter);
        }

        public Task RemoveExpired()
        {
            return Remove(x => x.Expiration < DateTime.UtcNow);
        }
    }
}
