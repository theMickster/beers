using Asp.Versioning;
using Beers.Application.Data;
using Beers.Application.Exceptions;
using Beers.Application.Interfaces.Data;
using Beers.Application.Validators.Brewer;
using Beers.Common.Attributes;
using Beers.Common.Constants;
using Beers.Common.Settings;
using Beers.Domain.Profiles;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Beers.API.libs;

[ExcludeFromCodeCoverage]
internal static class RegisterServices
{
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Because we said so.")]
    internal static WebApplicationBuilder RegisterAspDotNetServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
                options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
            })
            .AddJsonOptions(options => options.JsonSerializerOptions.WriteIndented = true)
            .AddXmlSerializerFormatters()
            .AddXmlDataContractSerializerFormatters();

        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version"));
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Beers API",
                    Version = "v1",
                    Description = "A web api for managing all things beer",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Bug Bashing Anonymous",
                        Url = new Uri("https://example.com/contact"),
                        Email = "bug.bashing.anonymous@example.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Bug Bashing Anonymous",
                        Url = new Uri("https://example.com/license")
                    }
                });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            options.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                {
                    return new[] { controllerActionDescriptor.ControllerName };
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            options.DocInclusionPredicate((name, api) => true);
        });

        builder.Services.AddAutoMapper(typeof(BeerTypeEntityToModelProfile).GetTypeInfo().Assembly);
        builder.Services.AddValidatorsFromAssemblyContaining<CreateBrewerValidator>();
        return builder;
    }

    internal static WebApplicationBuilder RegisterDataServices(this WebApplicationBuilder builder)
    {
        var cosmosSettings = builder.Configuration.GetSection(CosmosDbConnectionSettings.SettingsRootName).Get<CosmosDbConnectionSettings>() ??
                             throw new ConfigurationException("The required Configuration settings keys for the Azure Cosmos Db Settings are missing. Please verify configuration.");
        
        builder.Services.TryAddSingleton(factory =>
        {
            var cosmosContainers = new List<(string, string)>
            {
                (cosmosSettings.DatabaseName!, CosmosContainerConstants.MainContainer),
                (cosmosSettings.DatabaseName!, CosmosContainerConstants.MetadataContainer)
            };

            var cosmosClient = CosmosClient.CreateAndInitializeAsync(cosmosSettings.Account, cosmosSettings.SecurityKey, cosmosContainers)
                                    .GetAwaiter()
                                    .GetResult();
            
            return cosmosClient;
        });

        builder.Services.AddDbContext<BeersMetadataDbContext>(
            options =>
            {
                options.UseCosmos(cosmosSettings.Account, cosmosSettings.SecurityKey, cosmosSettings.DatabaseName );
#if DEBUG
                options.EnableSensitiveDataLogging();
#endif
            });
        
        builder.Services.AddDbContextFactory<BeersDbContext>((serviceProvider, options) =>
        {
            options.UseCosmos(cosmosSettings.Account, cosmosSettings.SecurityKey, cosmosSettings.DatabaseName);
#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
        }, ServiceLifetime.Scoped);

        builder.Services.AddScoped<IBeersMetadataDbContext>(
            provider => provider.GetService<BeersMetadataDbContext>() ??
                        throw new ConfigurationException("The BeersMetadataDbContext is not properly registered in the correct order."));

        return builder;
    }

    internal static WebApplicationBuilder RegisterServicesViaReflection(this WebApplicationBuilder builder)
    {
        var scoped = typeof(ServiceLifetimeScopedAttribute);
        var transient = typeof(ServiceLifetimeTransientAttribute);
        var singleton = typeof(ServiceLifetimeSingletonAttribute);

        var appServices = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.ManifestModule.Name.StartsWith("Beers."))

            .SelectMany(t => t.GetTypes())
            .Where(x => (x.IsDefined(scoped, false) ||
                         x.IsDefined(transient, false) ||
                         x.IsDefined(singleton, false)) && !x.IsInterface)
            .Select(y => new { InterfaceName = y.GetInterface($"I{y.Name}"), Service = y })
            .Where(z => z.InterfaceName != null)
            .ToList();

        appServices.ForEach(t =>
        {
            if (t.Service.IsDefined(scoped, false))
            {
                builder.Services.AddScoped(t.InterfaceName!, t.Service);
            }

            if (t.Service.IsDefined(transient, false))
            {
                builder.Services.AddTransient(t.InterfaceName!, t.Service);
            }

            if (t.Service.IsDefined(singleton, false))
            {
                builder.Services.AddSingleton(t.InterfaceName!, t.Service);
            }
        });
        return builder;
    }

    internal static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
    {
        var builder = new ServiceCollection()
            .AddLogging()
            .AddMvc()
            .AddNewtonsoftJson()
            .Services.BuildServiceProvider();

        return builder
            .GetRequiredService<IOptions<MvcOptions>>()
            .Value
            .InputFormatters
            .OfType<NewtonsoftJsonPatchInputFormatter>()
            .First();
    }
}
