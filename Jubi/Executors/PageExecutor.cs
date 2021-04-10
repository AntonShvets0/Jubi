using Jubi.Abstracts;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;

namespace Jubi.Executors
{
    public class PageExecutor : CommandExecutor
    {
        public override string Alias { get; } = "page";
        public override bool IsHidden { get; } = true;

        public override Message? Execute(User user, string[] args)
        {
            if (args.Length == 1)
            {
                if (args[0] == "next")
                {
                    if (user.Keyboard.Pages.Count - 1 < user.KeyboardPage + 1) return null;
                    user.KeyboardPage++;
                }
                else
                {
                    if (user.Keyboard.Pages.Count == 1) return null;
                    user.KeyboardPage--;
                }
            }

            return new Message(null, 
                new ReplyMarkupKeyboard(user.Keyboard, user.KeyboardPage));
        }
    }
}