using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Telegram.Api;
using Jubi.Telegram.Exceptions;

namespace Jubi.Telegram
{
    public class TelegramProvider : SiteProvider<TelegramUser>
    {
        public override IApiProvider Api { get; set; }
        public override string Id { get; set; } = "telegram";

        public override void OnInit()
        {
            base.OnInit();
            Api = new TelegramApiProvider(AccessToken);
        }
    }
}