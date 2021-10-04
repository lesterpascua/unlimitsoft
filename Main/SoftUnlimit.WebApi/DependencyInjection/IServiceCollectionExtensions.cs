﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.AutoMapper;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.Data.EntityFramework.Configuration;
using SoftUnlimit.Data.EntityFramework.DependencyInjection;
using SoftUnlimit.Json;
using SoftUnlimit.Reflection;
using SoftUnlimit.Security;
using SoftUnlimit.Web.AspNet.Filter;
using SoftUnlimit.WebApi.Sources.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SoftUnlimit.WebApi.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Load standard configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="corsOrigin"></param>
        /// <param name="databaseSettings"></param>
        ///// <param name="authorizeSettings">Security configuration setting.</param>
        /// <param name="filterRequestLoggerSettings"></param>
        /// <param name="filterValidationSettings"></param>
        /// <param name="filterTransformResponseOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfiguration(this IServiceCollection services,
            IConfiguration configuration,
            out string[] corsOrigin,
            //out ServiceSettings serviceSettings,
            out DatabaseSettings databaseSettings,
            //out AuthorizeSettings authorizeSettings,
            out RequestLoggerAttribute.Settings filterRequestLoggerSettings,
            out ValidationModelAttribute.Settings filterValidationSettings,
            out TransformResponseAttributeOptions filterTransformResponseOptions
        )
        {
            corsOrigin = configuration.GetSection("AllowedCors").Get<string[]>();

            var databaseSection = configuration.GetSection("Database");
            //var authorizeSection = configuration.GetSection("Authorize");

            var filterRequestLoggerSection = configuration.GetSection("Filter:RequestLogger");
            var filterValidationSection = configuration.GetSection("Filter:Validation");
            var filterTransformResponseSection = configuration.GetSection("Filter:TransformResponse");


            services.Configure<DatabaseSettings>(databaseSection);
            //services.Configure<AuthorizeSettings>(authorizeSection);

            services.Configure<RequestLoggerAttribute.Settings>(filterRequestLoggerSection);
            services.Configure<ValidationModelAttribute.Settings>(filterValidationSection);
            services.Configure<TransformResponseAttributeOptions>(filterTransformResponseSection);


            databaseSettings = databaseSection.Get<DatabaseSettings>();
            //authorizeSettings = authorizeSection.Get<AuthorizeSettings>();

            filterRequestLoggerSettings = filterRequestLoggerSection.Get<RequestLoggerAttribute.Settings>();
            filterValidationSettings = filterValidationSection.Get<ValidationModelAttribute.Settings>();
            filterTransformResponseOptions = filterTransformResponseSection.Get<TransformResponseAttributeOptions>();

            return services;
        }

        /// <summary>
        /// Register:
        /// <see cref="MyIdGenerator"/>, 
        /// <see cref="CQRS.Command.ICommandHandler"/>,
        /// <see cref="CQRS.Command.ICommandDispatcher"/>,
        /// <see cref="CQRS.Query.IQueryHandler"/>,
        /// <see cref="CQRS.Query.IQueryDispatcher"/>,
        /// <see cref="CQRS.Event.IEventHandler"/>,
        /// <see cref="CQRS.Event.IEventDispatcher"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="serviceId"></param>
        /// <param name="cqrsSettings"></param>
        /// <returns></returns>
        public static IServiceCollection AddCQRS(this IServiceCollection services,
            ushort serviceId,
            IEnumerable<UnitOfWorkSettings> unitOfWorkSettings,
            CQRSSettings cqrsSettings)
        {
            var gen = new MyIdGenerator(serviceId);
            services.AddSingleton<IMyIdGenerator>(gen);
            services.AddSingleton<IServiceMetadata>(gen);

            if (unitOfWorkSettings?.Any() == true)
                foreach (var unitOfWorkSetting in unitOfWorkSettings)
                    services.AddSoftUnlimitDefaultFrameworkUnitOfWork(unitOfWorkSetting);

            if (cqrsSettings != null)
            {
                if (cqrsSettings.PreeDispatchAction == null)
                    cqrsSettings.PreeDispatchAction = (provider, command) => {
                        //var jnCommand = (Command)command;
                        //UpdateTraceAndCorrelation(provider, jnCommand.Props.User.TraceId, jnCommand.Props.User.CorrelationId);
                    };
                services.AddSoftUnlimitDefaultCQRS(cqrsSettings);
            }
            return services;
        }

        /// <summary>
        /// Register automapper for the entity.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IServiceCollection AddMapper(this IServiceCollection services, Assembly[] assemblies, Action<IServiceProvider, IMapperConfigurationExpression> setup = null)
        {
            services.AddSingleton<IMapper>(provider => new Mapper(new MapperConfiguration(config => {
                config.AllowNullCollections = true;
                config.AllowNullDestinationValues = true;

                config.AddDeepMaps(assemblies);
                config.AddCustomMaps(assemblies);

                config.ConstructServicesUsing(t =>
                {
                    var converter = provider.GetService(t);
                    if (converter != null)
                        return converter;

                    return t.CreateInstance(provider);
                });

                setup?.Invoke(provider, config);
            })));
            services.AddSingleton<Map.IMapper, AutoMapperObjectMapper>();

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="services"></param>
        /// <param name="corsOrigin">Defined Cors domains allowd. </param>
        /// <param name="useRequestLoggerAttribute">Logger all request.</param>
        /// <param name="corsPolicy">Name used in cors policy. </param>
        /// <param name="authorizeRequire">Require autorization for all endpoints.</param>
        /// <param name="addViewToController"></param>
        /// <param name="useNewtonsoft"></param>
        /// <param name="mvcBuilderOption">Extra configuration for standard.</param>
        /// <param name="mvcOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddAspNet<TStartup>(this IServiceCollection services,
            string[] corsOrigin,
            bool useRequestLoggerAttribute,
            string corsPolicy = "default",
            bool authorizeRequire = true,
            bool addViewToController = false,
            bool useNewtonsoft = true,
            Action<IMvcBuilder> mvcBuilderOption = null,
            Action<MvcOptions> mvcOptions = null
        )
            where TStartup : class
        {
            //services.AddLocalization(options => options.ResourcesPath = "Resources");
            // 
            // uncomment, if you want to add an MVC-based UI
            void defaultMvcOptions(MvcOptions options)
            {
                if (authorizeRequire)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                }

                if (useRequestLoggerAttribute)
                    options.Filters.Add(typeof(RequestLoggerAttribute));

                options.Filters.Add(typeof(ValidationModelAttribute));
                options.Filters.Add(typeof(TransformResponseAttribute));
            }
            var mvcBuilder = addViewToController ? services.AddControllersWithViews(mvcOptions ?? defaultMvcOptions) : services.AddControllers(mvcOptions ?? defaultMvcOptions);
            //mvcBuilder
            //.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            //.AddDataAnnotationsLocalization()
            //.AddFluentValidation(options => options.RegisterValidatorsFromAssemblyContaining<TStartup>());
            if (useNewtonsoft)
            {
                //mvcBuilder.AddNewtonsoftJson(setup => {
                //    setup.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                //});
            }
            else
            {
                mvcBuilder.AddJsonOptions(setup =>
                {
                    setup.JsonSerializerOptions.WriteIndented = false;
                    setup.JsonSerializerOptions.IgnoreNullValues = true;
                });
            }

            mvcBuilderOption?.Invoke(mvcBuilder);

            if (corsPolicy?.Any() == true)
                services.AddCors(options => {
                    options.AddPolicy(corsPolicy, policy => {            // this defines a CORS policy called "{corsPolicy}"
                        policy.SetIsOriginAllowed(origin => {
                            if (corsOrigin == null)
                                return false;
                            return corsOrigin.Contains("*") || corsOrigin.Contains(origin);
                        })
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
                });

            return services;
      }
    }
}
