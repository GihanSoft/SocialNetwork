using System;
using System.Text;
using System.Threading.Tasks;

using Gihan.Identity.EfCore;
using Gihan.Identity.Models;
using Gihan.Identity.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Gihan.Identity;

public static class Configure
{
    public static void AddJwtIdentity<TDbContext, TUser>(
        this IServiceCollection services,
        JwtIdentityOptions jwtOptions)
        where TDbContext : IdentityDbContext<TUser>
        where TUser : UserBase
    {
        services.AddDbContext<IdentityDbContext<TUser>, TDbContext>();
        services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<TUser>, Microsoft.AspNetCore.Identity.PasswordHasher<TUser>>();
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
            config.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtOptions.Issuer,
                ValidateIssuer = false,
                ValidAudience = jwtOptions.Audience,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                TokenDecryptionKey = jwtOptions.UseEncryption
                    ? new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                    : null
            };
            config.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>().CreateLogger(nameof(JwtBearerEvents));
                    logger.LogError(context.Exception, "Authentication failed");
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
                    logger.LogError("OnChallenge error. {error}, {description}", context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });
    }
}
