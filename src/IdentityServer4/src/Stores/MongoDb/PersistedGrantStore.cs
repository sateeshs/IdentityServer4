// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.MongoDB.Interfaces;
using IdentityServer4.MongoDB.Mappers;
using Microsoft.Extensions.Logging;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;

namespace IdentityServer4.Stores.MongoDB
{
    public class MongoDbPersistedGrantStore : IPersistedGrantStore
    {
        private readonly IPersistedGrantDbContext _context;
        private readonly ILogger<MongoDbPersistedGrantStore> _logger;

        public MongoDbPersistedGrantStore(IPersistedGrantDbContext context, ILogger<MongoDbPersistedGrantStore> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task StoreAsync(PersistedGrant token)
        {
            try
            {
                _logger.LogDebug("Try to save or update {persistedGrantKey} in database", token.Key);
                await _context.InsertOrUpdate(t => t.Key == token.Key, token);
                _logger.LogDebug("{persistedGrantKey} stored in database", token.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Exception storing persisted grant");
                throw;
            }
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = _context.PersistedGrants.FirstOrDefault(x => x.Key == key);
            var model = persistedGrant;

            _logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return Task.FromResult(model);
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = _context.PersistedGrants.Where(x => x.SubjectId == subjectId).ToList();
            var model = persistedGrants.Select(x => x);

            _logger.LogDebug("{persistedGrantCount} persisted grants found for {subjectId}", persistedGrants.Count, subjectId);

            return Task.FromResult(model);
        }

        public Task RemoveAsync(string key)
        {
            _logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

            _context.Remove(x => x.Key == key);
            
            return Task.FromResult(0);
        }

        public Task RemoveAllAsync(string subjectId, string clientId)
        {
            _logger.LogDebug("removing persisted grants from database for subject {subjectId}, clientId {clientId}", subjectId, clientId);

            _context.Remove(x => x.SubjectId == subjectId && x.ClientId == clientId);
            
            return Task.FromResult(0);
        }

        public Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            _logger.LogDebug("removing persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}", subjectId, clientId, type);

            _context.Remove(
               x =>
               x.SubjectId == subjectId &&
               x.ClientId == clientId &&
               x.Type == type);

            return Task.FromResult(0);
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            filter.Validate();

            var items = Filter(filter);

            return Task.FromResult(items);
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            _context.Remove(x=>x.SubjectId == filter.SubjectId && x.ClientId == filter.ClientId);

            return Task.CompletedTask;
        }
        private IEnumerable<PersistedGrant> Filter(PersistedGrantFilter filter)
        {
            var query =_context.PersistedGrants;

            if (!String.IsNullOrWhiteSpace(filter.ClientId))
            {
                query = query.Where(x => x.ClientId == filter.ClientId);
            }
            //if (!String.IsNullOrWhiteSpace(filter.SessionId))
            //{
            //    query = query.Where(x => x.SessionId == filter.SessionId);
            //}
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x => x.Type == filter.Type);
            }

            var items = query.ToArray().AsEnumerable();
            return items;
        }
    }
}