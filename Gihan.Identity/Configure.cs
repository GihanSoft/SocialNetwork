using SocialNetwork.Identity.EfCore;
using SocialNetwork.Identity.Models;
using SocialNetwork.Identity.Services;
using SocialNetwork.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SocialNetwork.Identity
{
    public static class Configure
    {
        public static void AddJwtIdentity<TDbContext, TUser>(
            this IServiceCollection services,
            JwtIdentityOptions jwtOptions,
            CryptographyOptions cryptographyOptions)
            where TDbContext : IdentityDbContext<TUser>
            where TUser : UserBase
        {
            services.AddDbContext<IdentityDbContext<TUser>, TDbContext>();
            services.AddScoped<IHasher, Hasher>(s=> new Hasher(SHA256.Create()));
            services.AddScoped<IUserManager<TUser>, UserManager<TUser>>();
            services.AddScoped<IJwtAuthManager<TUser>, JwtAuthManager<TUser>>();
            services.AddHttpContextAccessor();
            services.TryAddSingleton(p => jwtOptions);

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(config =>
            {
                config.RequireHttpsMetadata = false;
                config.SaveToken = true;
                var keyProvider = new KeyProvider(jwtOptions.SecretKey, cryptographyOptions.KeySize);
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateIssuer = false,
                    ValidAudience = jwtOptions.Audience,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(keyProvider.GetNextKey(256)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    TokenDecryptionKey = jwtOptions.UseEncryption ?
                                         new SymmetricSecurityKey(keyProvider.Key.ToArray()) : null
                };
                config.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        logger.LogError("Authentication failed.", context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var tokenValidatorService = context.HttpContext.RequestServices
                            .GetRequiredService<IJwtAuthManager<TUser>>();
                        return tokenValidatorService.ValidateAsync(context).AsTask();
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                        logger.LogError("OnChallenge error", context.Error, context.ErrorDescription);
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
