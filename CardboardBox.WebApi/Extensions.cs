using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace CardboardBox.WebApi
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Setup;

    public static class Extensions
    {
        public static IServiceProvider Map(this IServiceCollection collection, Action<IDependencyHandle> map = null)
        {
            var col = collection.CardboardBox();

            map?.Invoke(col);
                         
            return col.Build<IServiceProvider>();
        }

        public static IServiceCollection Jwt(this IServiceCollection collection, string issuer, string key)
        {
            collection
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(_ =>
                {
                    _.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = issuer,
                        IssuerSigningKey = new JwtToken(key).Key
                    };
                });

            return collection;
        }

        public static IDependencyHandle AddNLog(this IDependencyHandle map)
        {
            var logger = new ServiceCollection()
                .AddLogging(_ =>
                {
                    _.SetMinimumLevel(LogLevel.Debug);
                    _.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true
                    });
                })
                .BuildServiceProvider()
                .GetRequiredService<ILogger<JwtToken>>();

            map.Use<ILogger>(logger);
            return map;
        }
    }
}
