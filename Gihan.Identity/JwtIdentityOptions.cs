using System;

namespace SocialNetwork.Identity
{
    public class JwtIdentityOptions
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { set; get; }
        public TimeSpan ValidityTime { get; set; }
        public bool UseEncryption { get; set; } = false;
        public bool AllowMultipleLogins { get; set; } = true;
        public bool AllowKickOthers { get; set; } = true;
    }
}
