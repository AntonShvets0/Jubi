using Jubi.Updates;
using Jubi.VKontakte.Enums;

namespace Jubi.VKontakte.Updates
{
    public class LikeContent : IUpdateContent
    {
        public int PostId { get; set; }
        
        public VKontakteObjectType ObjectType { get; set; }
        
        public long ObjectOwnerId { get; set; }
        
        public long ObjectId { get; set; }
        
        public LikeType Type { get; set; }
    }
}