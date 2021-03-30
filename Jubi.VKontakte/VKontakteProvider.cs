using Jubi.Abstracts;
using Jubi.Api;
using Jubi.VKontakte.Api;
using Jubi.VKontakte.Exceptions;

namespace Jubi.VKontakte
{
    public class VKontakteProvider : SiteProvider<VKontakteUser>
    {
        protected sealed override IApiProvider Api { get; set; }
        public override string Id { get; set; } = "vkontakte";

        public override void OnInit()
        {
            base.OnInit();
            Api = new VKontakteApiProvider(AccessToken);
        }
    }
}