using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static System.Text.Encoding;

namespace SocialNetwork.Security.Cryptography
{
    public interface ICryptographer : IDisposable
    {
        KeySize KeySize { get; }
        string Password { get; }

        string Decrypt(byte[] cipherTextBytes, bool hasSalt = true);
        ValueTask<string> DecryptAsync(byte[] cipherTextBytes, bool hasSalt = true);
        byte[] Encrypt(string plainText, bool hasSalt = true);
        ValueTask<byte[]> EncryptAsync(string plainText, bool hasSalt = true);
    }

    public class Cryptographer : ICryptographer
    {
        private readonly RandomNumberGenerator rng;

        protected KeyProvider KeyProvider { get; }
        protected byte SaltSize { get; }

        public string Password => KeyProvider.Password;
        public KeySize KeySize => (KeySize)KeyProvider.KeySize;

        public Cryptographer(string password, KeySize keySize = KeySize._256, byte saltSize = 7)
        {
            if (saltSize == 0) throw
                 new ArgumentException("0 salt length is forbidden", nameof(saltSize));

            rng = RandomNumberGenerator.Create();
            SaltSize = saltSize;
            KeyProvider = new KeyProvider(password, keySize, KeySize._128);
        }

        public Cryptographer(CryptographyOptions options)
            :this(options.Password, options.KeySize, options.SaltSize)
        {
        }

        public async ValueTask<byte[]> EncryptAsync(string plainText, bool hasSalt = true)
        {
            if (string.IsNullOrEmpty(plainText)) return Array.Empty<byte>();
            // adding a salt to prevent same result in same plain texts
            if (hasSalt)
                plainText = ASCII.GetString(rng.GetNonZeroRandomBytes(SaltSize)) + plainText;

            using Aes aes = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                new AesCng() as Aes : new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Key = KeyProvider.Key.ToArray();
            aes.IV = KeyProvider.IV.ToArray();

            using var memoryStream = new MemoryStream();
            using var cryptoStream =
                new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            var plainTextBytes = Unicode.GetBytes(plainText);
            await cryptoStream.WriteAsync(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.Close();
            return memoryStream.ToArray();
        }
        public byte[] Encrypt(string plainText, bool hasSalt = true)
            => EncryptAsync(plainText, hasSalt).Result;

        public async ValueTask<string> DecryptAsync(byte[] cipherTextBytes, bool hasSalt = true)
        {
            if (cipherTextBytes.Length == 0) return string.Empty;

            string plainText = null;
            using Aes aes = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                new AesCng() as Aes : new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Key = KeyProvider.Key.ToArray();
            aes.IV = KeyProvider.IV.ToArray();
            using var memoryStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream =
                new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream, Unicode);
            plainText = await reader.ReadToEndAsync();
            cryptoStream.Close();

            // removing salt value.
            if (hasSalt)
                plainText = plainText.Substring(SaltSize);
            return plainText;
        }
        public string Decrypt(byte[] cipherTextBytes, bool hasSalt = true)
            => DecryptAsync(cipherTextBytes, hasSalt).Result;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                KeyProvider.Dispose();
                rng.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Cryptographer()
        {
            Dispose(true);
        }
    }
}
