
using System;
using System.Security.Cryptography;

namespace SocialNetwork.Security.Cryptography
{
#if NETCOREAPP3_0 || NETCOREAPP3_1
    using Microsoft.Extensions.DependencyInjection;
    public static class Configure
    {
        public static void AddCryptography(this IServiceCollection services,
            CryptographyOptions cryptographyOptions)
        {
            services.AddTransient(p => cryptographyOptions);
            services.AddTransient<ICryptographer, Cryptographer>();
            services.AddTransient<HashAlgorithm, SHA256>(p => SHA256.Create());
            services.AddTransient<IHasher, Hasher>();
        }

        public static void AddCryptography(this IServiceCollection services,
            Action<CryptographyOptions> cryptographyOptionsBuilder)
        {
            var options = new CryptographyOptions();
            cryptographyOptionsBuilder(options);
            services.AddCryptography(options);
        }

        public static void AddCryptoGraphy(this IServiceCollection services, string password)
        {
            services.AddCryptography(options => options.Password = password);
        }
    }
#endif

    public class CryptographyOptions
    {
        public string Password { get; set; }
        public KeySize KeySize { get; set; } = KeySize._256;
        public byte SaltSize { get; set; } = 7;
    }
}
