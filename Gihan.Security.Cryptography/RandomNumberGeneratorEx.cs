using System.Security.Cryptography;

namespace SocialNetwork.Security.Cryptography
{
    public static class RandomNumberGeneratorEx
    {
        public static byte[] GetRandomBytes(this RandomNumberGenerator rng, int length)
        {
            var arr = new byte[length];
            rng.GetBytes(arr);
            return arr;
        }

        public static byte[] GetNonZeroRandomBytes(this RandomNumberGenerator rng, int length)
        {
            var arr = new byte[length];
            rng.GetNonZeroBytes(arr);
            return arr;
        }
    }
}
