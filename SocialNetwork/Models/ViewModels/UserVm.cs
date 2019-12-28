using SocialNetwork.EfCore;

namespace SocialNetwork.Models.ViewModels
{
    public class UserVm : EntityBase<int>
    {
        //public string AvatarUrl { get; set; }
        public string UserName { get; set; }
        public bool IsPrivate { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        //public IEnumerable<PostVm> Posts { get; set; }
        public string PostsUrl { get; set; }
        public bool IsFollowed { get; set; }
        public bool IsFollowRequested { get; set; }
        public long FollowersCount { get; set; }
        public long FollowingsCount { get; set; }

    }
}
