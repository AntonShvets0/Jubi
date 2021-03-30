using Jubi.Abstracts;
using Jubi.Api;
using Jubi.VKontakte.Api;
using Jubi.VKontakte.Exceptions;

namespace Jubi.VKontakte
{
    public class VKontakteProvider : SiteProvider<VKontakteUser>
    {
        protected sealed override IApiProvider Api { get; set; }
        public override string Id { get; set; } = "VK";

        public override void OnInit()
        {
            if (BotInstance.Configuration["apiKeys"]?["vkontakte"] == null)
                throw new VKontakteProviderException("VKontakte api key not found");

            Api = new VKontakteApiProvider(BotInstance.Configuration["apiKeys"]["vkontakte"]);
        }
    }
}