﻿using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Beers.Common.Settings;
using Beers.Common.Constants;
using Microsoft.Azure.Cosmos;

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
                new HeaderApiVersionReader("x-api-version"),
                new MediaTypeApiVersionReader("x-api-version"));
        });

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                MakeOpenApiInfo("Beers API",
                    "v1",
                    "API",
                    new Uri("http://hello-world.info")));

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

        return builder;
    }

    internal static WebApplicationBuilder RegisterDataServices(this WebApplicationBuilder builder)
    {

        builder.Services.TryAddSingleton(factory =>
        {
            var cosmosSettings = factory.GetRequiredService<CosmosDbConnectionSettings>();
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

        return builder;
    }

    #region Private Methods
    private static OpenApiInfo MakeOpenApiInfo(string title, string version, string description, Uri releaseNotes)
    {
        var oai = new OpenApiInfo
        {
            Title = title,
            Version = version,
            Contact = new OpenApiContact
                { Email = "bug.bashing.anonymous@outlook.com", Name = "Bug Bashing Anonymous" },
            Description = description
        };

        if (releaseNotes != null)
        {
            oai.Contact.Url = releaseNotes;
        }

        return oai;
    }

    #endregion
}
