using System;
using System.Security.Cryptography;
using System.Text;

namespace SocialNetwork.Security.Cryptography
{
    public interface IHasher : IDisposable
    {
        string Hash(string input);
    }

    public class Hasher : IDisposable, IHasher
    {
        public static string Hash(HashAlgorithm algorithm, string input)
        {
            var bytes = Encoding.Unicode.GetBytes(input);
            var hashedBytes = algorithm.ComputeHash(bytes);
            var base64Hash = Convert.ToBase64String(hashedBytes);
            return base64Hash;
        }

        private readonly HashAlgorithm _hashAlgorithm;

        public Hasher(HashAlgorithm algorithm) => _hashAlgorithm = algorithm;

        public string Hash(string input) => Hash(_hashAlgorithm, input);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _hashAlgorithm.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
