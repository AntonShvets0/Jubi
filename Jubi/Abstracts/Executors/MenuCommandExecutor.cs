using System;
using System.Linq;
using System.Reflection;
using Jubi.Abstracts.MenuControlExecutors;
using Jubi.Attributes;
using Jubi.Exceptions;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;

namespace Jubi.Abstracts.Executors
{
    public abstract class MenuCommandExecutor : CommandExecutor
    {
        protected virtual int MaxButtonsInRow { get; } = 1;

        public override Message? Execute()
        {
            var markup = Parent == null
                ? new ReplyMarkupKeyboard()
                : new ReplyMarkupKeyboard(false, () => ExecuteMarkup(Parent));
            markup.MaxInRows = MaxButtonsInRow;

            foreach (var executor in Subcommands)
            {
                if (executor is NewLine)
                {
                    markup.AddLine();
                    continue;
                }
                
                executor.User = User;
                executor.Parent = this;
                executor.Args = Array.Empty<object>();

                var isMiddlewaresReturnError = false;
                
                foreach (var middleware in executor.Middlewares)
                {
                    try
                    {
                        if (!middleware(executor))
                        {
                            isMiddlewaresReturnError = true;
                            break;
                        }
                    }
                    catch (JubiException)
                    {
                        isMiddlewaresReturnError = true;
                        break;
                    }
                }

                if (isMiddlewaresReturnError) continue;
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