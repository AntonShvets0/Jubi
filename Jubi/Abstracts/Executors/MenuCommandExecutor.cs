using System;
using System.Linq;
using System.Reflection;
using Jubi.Attributes;
using Jubi.Exceptions;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;

namespace Jubi.Abstracts.Executors
{
    public abstract class MenuCommandExecutor : CommandExecutor
    {
        public override Message? Execute()
        {
            var markup = Parent == null
                ? new ReplyMarkupKeyboard(true)
                : new ReplyMarkupKeyboard(true, () => ExecuteMarkup(Parent));
            markup.MaxInRows = 1;

            foreach (var executor in Subcommands)
            {
                executor.User = User;
                executor.Parent = this;
                executor.Args = Array.Empty<object>();
                
                foreach (var middleware in executor.Middlewares)
                {
                    if (!middleware(executor)) continue;
                }

                markup.AddButton(executor.Alias, () => ExecuteMarkup(executor));
            }

            return markup;
        }

        private void ExecuteMarkup(CommandExecutor executor)
        {
            var response = executor.Execute();
            if (response == null) return;
                    
            User.Send((Message)response);
        }
    }
}