using System.Collections.Generic;
using Jubi.VKontakte.Models.Enums;

namespace Jubi.VKontakte.Models
{
    public class VKontakteGroupInfo
    {
        public ulong Id { get; set; }
        
        public string Name { get; set; }
        
        public string ScreenName { get; set; }
        
        public VisibilityGroupType VisibilityGroupType { get; set; }

        public DeactivatedType DeactivatedType { get; set; }

        public GroupType GroupType { get; set; }

        public string[] Cover { get; set; }
    }
}