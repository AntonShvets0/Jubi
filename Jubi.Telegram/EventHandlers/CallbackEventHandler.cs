using System.Linq;
using Jubi.Abstracts;
using Jubi.EventHandlers;
using Jubi.Telegram.Api.Types;
using Jubi.Telegram.Updates;
using Jubi.Updates;
using Jubi.Updates.Types;

namespace Jubi.Telegram.EventHandlers
{
    public class CallbackEventHandler : EventHandler<CallbackQueryContent>
    {
        public override void Handle(User initiator, CallbackQueryContent data)
        {
            initiator.QueryId = data.Id;
            initiator.Provider.EmulateExecute(initiator, data.Data, data.PeerId);
            (SiteProvider.Api.Messages as TelegramMessageApiProvider).AnswerCallbackQuery(data.Id);
        }
    }
}