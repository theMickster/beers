using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace Beers.API.libs;

[ExcludeFromCodeCoverage]
internal static class RegisterAuthentication
{
    internal static WebApplicationBuilder RegisterAppAuthentication(this WebApplicationBuilder builder)
    {

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));


        return builder;
    }
}
