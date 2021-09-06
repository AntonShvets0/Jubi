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

        public virtual CommandExecutor[] InlineSubcommands { get; set; }

        public override Message? Execute()
        {
            ReplyMarkupKeyboard markup = null;
            InlineMarkupKeyboard inline = null;
            
            if (InlineSubcommands != null)
            {
                inline = new InlineMarkupKeyboard();

                foreach (var inlineSubcommand in InlineSubcommands)
                {
                    if (inlineSubcommand is NewLine)
                    {
                        inline.AddLine();
                        continue;
                    }

                    inlineSubcommand.User = User;
                    inlineSubcommand.Parent = this;
                    inlineSubcommand.Args = Array.Empty<object>();

                    var isMiddlewaresReturnError = false;
                
                    foreach (var middleware in inlineSubcommand.Middlewares)
                    {
                        try
                        {
                            if (!middleware(inlineSubcommand))
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
                
                    inline.AddButton(inlineSubcommand.Alias, () => ExecuteMarkup(inlineSubcommand));
                }
            }

            if (Subcommands != null)
            {
                markup = Parent == null
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
            }

            return markup != null && inline != null
                ? new Message(null, markup, inline)
                : (inline != null ? new Message(null, inline) : markup);
        }

        private void ExecuteMarkup(CommandExecutor executor)
        {
            var response = executor.Execute();
            if (response == null) return;
                    
            User.Send((Message)response);
        }
    }
}