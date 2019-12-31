using SocialNetwork.EfCore;

namespace SocialNetwork.Models.ViewModels
{
    public class UserVm : AccountVm
    {
        public string PostsUrl { get; set; }
        public bool IsFollowed { get; set; }
        public bool IsFollowAccepted { get; set; }
        public bool IsFollowRequested { get; set; }
        public long FollowersCount { get; set; }
        public long FollowingsCount { get; set; }

    }
}
