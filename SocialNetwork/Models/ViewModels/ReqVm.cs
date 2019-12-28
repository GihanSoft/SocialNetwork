namespace SocialNetwork.Models.ViewModels
{
    public class ReqVm
    {
        public long? LastGuttedId { get; set; }
        public byte MaxCountToGet { get; set; } = 10;
        public bool TowardOlds { get; set; } = true;
    }
}
