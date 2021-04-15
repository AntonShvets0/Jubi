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
            if (Get<string>(0) == "next")
            {
                if (User.Keyboard.Pages.Count - 1 < User.KeyboardPage + 1) return null;
                User.KeyboardPage++;
            }
            else
            {
                if (User.Keyboard.Pages.Count == 1) return null;
                User.KeyboardPage--;
            }

            return new Message(null, 
                new ReplyMarkupKeyboard(User.Keyboard, User.KeyboardPage));
        }
    }
}