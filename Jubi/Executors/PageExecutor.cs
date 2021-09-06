using Jubi.Abstracts;
using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;

namespace Jubi.Executors
{
    [Command("page")]
    public class PageExecutor : CommandExecutor<string>
    {
        public override bool IsHidden { get; } = true;

        public override Message? Execute()
        {
            var chat = User.GetChat();
            if (Get<string>(0) == "next")
            {
                if (chat.ReplyMarkupKeyboard.Pages.Count - 1 < chat.KeyboardPage + 1) return null;
                chat.KeyboardPage++;
            }
            else
            {
                if (chat.KeyboardPage - 1 < 0) return null;
                chat.KeyboardPage--;
            }

            return new Message(null, 
                new ReplyMarkupKeyboard(chat.ReplyMarkupKeyboard as ReplyMarkupKeyboard, chat.KeyboardPage));
        }
    }
}