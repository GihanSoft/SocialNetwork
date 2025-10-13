using System;

using Gihan.Identity;
using Gihan.Security.Cryptography;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SocialNetwork.Data;
using SocialNetwork.Models;

var builder = WebApplication.CreateBuilder();

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<AppDbContext>("default", configureDbContextOptions: opt => opt.UseLazyLoadingProxies());

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