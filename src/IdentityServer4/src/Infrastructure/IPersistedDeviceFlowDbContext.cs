using IdentityServer4.MongoDB.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
namespace IdentityServer4.Infrastructure
{
    public interface IPersistedDeviceFlowDbContext
    {
        IQueryable<DeviceFlowCodes> DeviceFlowCodes { get; }

        Task Remove(Expression<Func<DeviceFlowCodes, bool>> filter);

        Task RemoveExpired();

        Task InsertOrUpdate(Expression<Func<DeviceFlowCodes, bool>> filter, DeviceFlowCodes entity);

    }
}
