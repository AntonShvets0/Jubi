using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Telegram.Api;
using Jubi.Telegram.Exceptions;

namespace Jubi.Telegram
{
    public class TelegramProvider : SiteProvider<TelegramUser>
    {
        protected override IApiProvider Api { get; set; }
        public override string Id { get; set; } = "T";

        public override void OnInit()
        {
            if (BotInstance.Configuration["apiKeys"]?["telegram"] == null)
                throw new TelegramProviderException("Telegram api key not found");

            Api = new TelegramApiProvider(BotInstance.Configuration["apiKeys"]["telegram"]);
        }
    }
}