using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SocialNetwork.Data;
using SocialNetwork.Identity;
using SocialNetwork.Models;

using SocialNetwork.Security.Cryptography;

const string DefaultConnectionName = "DefaultConnection";

var builder = WebApplication.CreateBuilder();

builder.AddServiceDefaults();

builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString(DefaultConnectionName)));

builder.Services.AddJwtIdentity<AppDbContext, User>(new JwtIdentityOptions
{
    SecretKey = "secret key for JWT",
    ValidityTime = TimeSpan.FromDays(32)
}, new CryptographyOptions
{
    Password = "secret password for JWT",
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(_ => { });

app.UseSpa(spa => spa.UseProxyToSpaDevelopmentServer(app.Configuration["services:ui:http:0"]));

app.MapDefaultEndpoints();
app.MapControllerRoute(
    name: "default",
    pattern: "API/{controller=Home}/{action=Index}/{id?}");

app.Run();