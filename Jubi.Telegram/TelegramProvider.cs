using Jubi.Abstracts;
using Jubi.Api;
using Jubi.EventHandlers;
using Jubi.Telegram.Api;
using Jubi.Telegram.EventHandlers;
using Jubi.Telegram.Exceptions;

namespace Jubi.Telegram
{
    public class TelegramProvider : SiteProvider<TelegramUser>
    {
        public override IApiProvider Api { get; set; }
        public override string Id { get; set; } = "telegram";

        public override EventHandler[] EventHandlers { get; } = {new MessageEventHandler(), new CallbackEventHandler()};

        public override void OnInit()
        {
            base.OnInit();
            
            Api = new TelegramApiProvider(AccessToken);
        }
    }
}