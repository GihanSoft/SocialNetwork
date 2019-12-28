using SocialNetwork.Identity.EfCore;
using SocialNetwork.Identity.Models;
using SocialNetwork.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SocialNetwork.Identity.Services
{
    public interface IJwtAuthManager<TUser> : IDisposable
        where TUser : UserBase
    {
        ValueTask<string> CreateJwtTokenAsync(TUser user, string displayName = null);
        ValueTask ValidateAsync(TokenValidatedContext context);
        ValueTask RemoveJtiAsync(Guid jti);
        ValueTask RemoveAsync(AccessToken token);
    }

    public class JwtAuthManager<TUser> : IJwtAuthManager<TUser>
        where TUser : UserBase
    {
        //protected Hasher _hasher;
        protected IUserManager<TUser> _userManager;
        protected IdentityDbContext<TUser> _jwtIdentityDb;
        public HttpContext _context;
        protected JwtIdentityOptions _identityOptions;

        public JwtAuthManager(
            IdentityDbContext<TUser> jwtIdentityDb,
            IUserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            JwtIdentityOptions identityOptions)
        {
            _jwtIdentityDb = jwtIdentityDb;
            _userManager = userManager;
            _context = contextAccessor.HttpContext;
            _identityOptions = identityOptions;
        }

        public async ValueTask<string> CreateJwtTokenAsync(TUser user, string displayName = null)
        {
            if (user.Id == default)
                user = await _userManager.FindAsync(user);
            if (user is null || user.Id == default)
                throw new Exception("user not found");
            if (!_identityOptions.AllowMultipleLogins)
            {
                if (_jwtIdentityDb.AccessTokens.ToArray().Any(token => token.UserId == user.Id))
                {
                    throw new Exception("this user has logged in before");
                }
            }
            var accessToken = new AccessToken()
            {
                Id = Guid.NewGuid(),
                ExpiresTime = DateTime.Now.Add(_identityOptions.ValidityTime),
                UserId = user.Id,
                LastLoginTime = DateTime.Now,
                LastLoginIp = _context.Connection.RemoteIpAddress.ToString(),
                LastLoginUserAgent = _context.Request.Headers["User-Agent"][0]
            };
            accessToken = (await _jwtIdentityDb.AddAsync(accessToken)).Entity;
            await _jwtIdentityDb.SaveChangesAsync();
            //return accessToken.Id.ToString();

            var keyProvider = new KeyProvider(_identityOptions.SecretKey, KeySize._256);
            var issuer = _identityOptions.Issuer ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName, ClaimValueTypes.String, issuer),
                new Claim(JwtRegisteredClaimNames.Jti, accessToken.Id.ToString(), ClaimValueTypes.String, issuer),
                new Claim(JwtRegisteredClaimNames.Iss, issuer, ClaimValueTypes.String, issuer),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64, issuer),
            };
            if (displayName != null)
                claims.Add(new Claim(ClaimTypes.Name, displayName, ClaimTypes.Name, issuer));
            claims.AddRange(_jwtIdentityDb.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));
            var key = new SymmetricSecurityKey(keyProvider.GetNextKey(256));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.Now;
            var encryptingCredentials = new EncryptingCredentials(new SymmetricSecurityKey(keyProvider.Key.ToArray()),
                                SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);
            var descriptor = new SecurityTokenDescriptor
            {
                Audience = _identityOptions.Audience,
                EncryptingCredentials = _identityOptions.UseEncryption ? encryptingCredentials : null,
                Expires = now.Add(_identityOptions.ValidityTime),
                IssuedAt = now,
                Issuer = issuer,
                NotBefore = now,
                SigningCredentials = creds,
                Subject = new ClaimsIdentity(claims),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(descriptor);
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(securityToken);
            keyProvider.Dispose();
            return tokenStr;
        }

        public async ValueTask ValidateAsync(TokenValidatedContext context)
        {
            var claimsIdentity = context.Principal.Identity as ClaimsIdentity;

            if (claimsIdentity?.Claims is null || !claimsIdentity.Claims.Any())
            {
                context.Fail("This is not our issued token.");
                return;
            }

            var jtiString = claimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (jtiString is null || !Guid.TryParse(jtiString, out var jti))
            {
                context.Fail("This is not our issued token.");
                return;
            }

            var token = await _jwtIdentityDb.AccessTokens.FirstOrDefaultAsync(t => t.Id == jti);
            if (token is null || token.ExpiresTime < DateTime.Now)
            {
                if (token.ExpiresTime < DateTime.Now)
                {
                    _jwtIdentityDb.Remove(token);
                    await _jwtIdentityDb.SaveChangesAsync();
                }
                context.Fail("Token is not valid");
            }

            token.LastLoginTime = DateTime.Now;
            token.LastLoginIp = _context.Connection.RemoteIpAddress.ToString();
            token.LastLoginUserAgent = _context.Request.Headers["User-Agent"][0];
            _jwtIdentityDb.Update(token);
            await _jwtIdentityDb.SaveChangesAsync();

            context.Success();
        }

        public async ValueTask RemoveAsync(AccessToken token)
        {
            _jwtIdentityDb.Remove(token);
            await _jwtIdentityDb.SaveChangesAsync();
        }
        public async ValueTask RemoveJtiAsync(Guid jti)
        {
            var token = await _jwtIdentityDb.AccessTokens.FirstOrDefaultAsync(t => t.Id == jti);
            await RemoveAsync(token);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~JwtAuthManager()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
