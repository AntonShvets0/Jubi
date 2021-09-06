using Jubi.Abstracts;
using Jubi.Api;
using Jubi.EventHandlers;
using Jubi.VKontakte.Api;
using Jubi.VKontakte.Exceptions;

namespace Jubi.VKontakte
{
    public class VKontakteProvider : SiteProvider<VKontakteUser>
    {
        public sealed override IApiProvider Api { get; set; }
        public override string Id { get; set; } = "vkontakte";

        public override EventHandler[] EventHandlers { get; } = {new MessageEventHandler()};

        public override void OnInit()
        {
            base.OnInit();
            
            Api = new VKontakteApiProvider(AccessToken);
        }
    }
}