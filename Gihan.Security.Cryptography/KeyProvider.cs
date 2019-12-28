using System;
using System.Security.Cryptography;

namespace SocialNetwork.Security.Cryptography
{
    /// <summary>
    /// Cryptography key builder.
    /// This class make a binary key for cryptography from a string password
    /// </summary>
    public class KeyProvider : IDisposable
    {
        protected const int SaltSize = 8;
        protected const int Iterations = 149;
        /// <summary>
        /// A .Net Standard class that make binary key
        /// </summary>
        protected Rfc2898DeriveBytes KeyBuilder { get; }

        /// <summary>
        /// Key size in bit. To get byte size, divide it by 8.
        /// </summary>
        public int KeySize { get; }
        public int ByteKeySize => KeySize / 8;

        /// <summary>
        /// Base password string to make binary key from it
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Initial Vector based on <see cref="Password"/>
        /// </summary>
        public ReadOnlyMemory<byte> IV { get; }

        /// <summary>
        /// Cryptography binary key created based on <see cref="Password"/>
        /// </summary>
        public ReadOnlyMemory<byte> Key { get; }

        /// <summary>
        /// Initialize a new instance of the <see cref="KeyProvider"/> class
        /// using string password and key size.
        /// </summary>
        /// <param name="password">string password to build other binary keys based on it</param>
        /// <param name="keySize"></param>
        public KeyProvider(string password, KeySize keySize, KeySize ivSize = Cryptography.KeySize._128)
        {
            if (string.IsNullOrEmpty(password)) throw
                 new ArgumentNullException("Password is null or empty", nameof(password));

            Password = password; //set for public view
            KeySize = (int)keySize; //set for public view
            KeyBuilder = new Rfc2898DeriveBytes(password, new byte[SaltSize], Iterations);
            KeyBuilder.Salt = KeyBuilder.GetBytes(SaltSize);
            IV = KeyBuilder.GetBytes((int)ivSize / 8);
            Key = KeyBuilder.GetBytes(ByteKeySize);
        }

        public byte[] GetNextKey(int size)
        {
            return KeyBuilder.GetBytes(size);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                KeyBuilder?.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~KeyProvider()
        {
            Dispose(true);
        }
    }
}
