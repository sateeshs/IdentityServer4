// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.MongoDB.Mappers;

namespace IdentityServer4.Stores.MongoDB
{
    public class ResourceStore : IResourceStore
    {
        private readonly IConfigurationDbContext _context;
        private readonly ILogger<ResourceStore> _logger;
        private readonly IEnumerable<IdentityResource> _identityResources;
        private readonly IEnumerable<ApiResource> _apiResources;
        private readonly IEnumerable<ApiScope> _apiScopes;

        public ResourceStore(IConfigurationDbContext context, ILogger<ResourceStore> logger,
            IEnumerable<IdentityResource> identityResources = null,
            IEnumerable<ApiResource> apiResources = null,
            IEnumerable<ApiScope> apiScopes = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
            if (identityResources?.HasDuplicates(m => m.Name) == true)
            {
                throw new ArgumentException("Identity resources must not contain duplicate names");
            }

            if (apiResources?.HasDuplicates(m => m.Name) == true)
            {
                throw new ArgumentException("Api resources must not contain duplicate names");
            }

            if (apiScopes?.HasDuplicates(m => m.Name) == true)
            {
                throw new ArgumentException("Scopes must not contain duplicate names");
            }

            _identityResources = identityResources ?? Enumerable.Empty<IdentityResource>();
            _apiResources = apiResources ?? Enumerable.Empty<ApiResource>();
            _apiScopes = apiScopes ?? Enumerable.Empty<ApiScope>();
        }

        public Task<ApiResource> FindApiResourceAsync(string name)
        {
            var apis =
                from apiResource in _context.ApiResources
                where apiResource.Name == name
                select apiResource;

            var api = apis.FirstOrDefault();

            if (api != null)
            {
                _logger.LogDebug("Found {api} API resource in database", name);
            }
            else
            {
                _logger.LogDebug("Did not find {api} API resource in database", name);
            }

            return Task.FromResult(api.ToModel());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var names = scopeNames.ToArray();
            //TODO Need revist query
            var apis = 
                from api in _context.ApiResources
                where api.Scopes.Where(x => names.Contains(x.Scope)).Any()
                select api;

            var results = apis.ToArray();
            var models = results.Select(x => x.ToModel()).ToArray();

            _logger.LogDebug("Found {scopes} API scopes in database", models.SelectMany(x => x.Scopes).Select(x => x));

            return Task.FromResult(models.AsEnumerable());
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var scopes = scopeNames.ToArray();

            var resources =
                from identityResource in _context.IdentityResources
                where scopes.Contains(identityResource.Name)
                select identityResource;

            var results = resources.ToArray();

            _logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

            return Task.FromResult(results.Select(x => x.ToModel()).ToArray().AsEnumerable());
        }

        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            throw new NotImplementedException();
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            var identity = _context.IdentityResources;

            var apis = _context.ApiResources;

            //var result = new Resources(
            //    identity.ToArray().Select(x => x.ToModel()).AsEnumerable(),
            //    apis.ToArray().Select(x => x.ToModel()).AsEnumerable());
            var result = new Resources(_identityResources, _apiResources, _apiScopes);


            _logger.LogDebug("Found {scopes} as all scopes in database", result.IdentityResources.Select(x => x.Name).Union(result.ApiResources.SelectMany(x => x.Scopes).Select(x => x)));

            return Task.FromResult(result);
        }
    }
}