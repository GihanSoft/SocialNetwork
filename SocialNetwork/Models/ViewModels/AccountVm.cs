using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SocialNetwork.Models.ViewModels
{
    public class AccountVm
    {
        [StringLength(450, MinimumLength = 4)]
        public string UserName { get; set; }
        [EmailAddress, StringLength(450)]
        public string Email { get; set; }
        [StringLength(10, MinimumLength = 10)]
        public string Mobile { get; set; }
        [StringLength(128, MinimumLength = 8)]
        public string OldPassword { get; set; }
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; }
        [Compare(nameof(Password))]
        public string PasswordConf { get; set; }
        public bool RememberMe { get; set; }
        public bool IsPrivate { get; set; }
    }
}
