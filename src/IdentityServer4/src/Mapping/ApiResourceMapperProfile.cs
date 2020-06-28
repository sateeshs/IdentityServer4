// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AutoMapper;

using IdentityServer4.MongoDB.Entities;

using System.Linq;

namespace IdentityServer4.MongoDB.Mappers
{
    /// <summary>
    /// AutoMapper configuration for API resource
    /// Between model and entity
    /// </summary>
    public class ApiResourceMapperProfile : Profile
    {
        /// <summary>
        /// <see cref="ApiResourceMapperProfile"/>
        /// </summary>
        public ApiResourceMapperProfile()
        {
            // entity to model
            CreateMap<ApiResource, Models.ApiResource>(MemberList.Destination)
                .ForMember(x => x.Properties,
                    opt => opt.MapFrom(src => src.Properties.ToDictionary(item => item.Key, item => item.Value)))
                .ForMember(x => x.ApiSecrets, opt => opt.MapFrom(src => src.ApiSecrets.Select(x => x)))
                .ForMember(x => x.Scopes, opt => opt.MapFrom(src => src.Scopes.Select(x => x.Name)))
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(src => src.UserClaims.Select(x => x.Type)))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Properties, opt => opt.MapFrom(src => src.Properties))
                .ForMember(x => x.ShowInDiscoveryDocument, opt => opt.MapFrom(src => src.ShowInDiscoveryDocument))
                ;
            CreateMap<ApiSecret, Models.Secret>(MemberList.Destination);
            CreateMap<ApiScope, Models.ApiScope>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims.Select(x => x.Type)))
                .ForMember(x=>x.Description, opt =>opt.MapFrom(src=>src.Description))
                .ForMember(x => x.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(x => x.Emphasize, opt => opt.MapFrom(src => src.Emphasize))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                ;

            // model to entity
            CreateMap<Models.ApiResource, ApiResource>(MemberList.Source)
                .ForMember(x => x.Properties,
                    opt => opt.MapFrom(src => src.Properties.ToDictionary(item => item.Key, item => item.Value)))
                .ForMember(x => x.ApiSecrets, opts => opts.MapFrom(src => src.ApiSecrets.Select(x => x)))
                .ForMember(x => x.Scopes, opts => opts.MapFrom(src => src.Scopes.Select(x => new ApiScope { Name =x,UserClaims = src.UserClaims.Select(y=> new ApiScopeClaim { Type = y }).ToList() })))
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiResourceClaim { Type = x })));
            CreateMap<Models.Secret, ApiSecret>(MemberList.Source);
            CreateMap<Models.ApiScope, ApiScope>(MemberList.Source)
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(src => src.UserClaims.Select(x => new ApiScopeClaim { Type = x })))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.DisplayName, opt => opt.MapFrom(src => src.DisplayName))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Required, opt => opt.MapFrom(src => src.Required))
                .ForMember(x => x.ShowInDiscoveryDocument, opt => opt.MapFrom(src => src.ShowInDiscoveryDocument))
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims))
                ;
        }
    }
}