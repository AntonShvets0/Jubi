using System.Collections.Generic;
using System.Linq;
using Jubi.Abstracts;
using Jubi.Chats;
using Jubi.Enums;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Attachments.Keyboard.Models;
using Jubi.Updates.Types;
using Newtonsoft.Json.Linq;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.EventHandlers
{
    public class MessageEventHandler : EventHandler<MessageNewContent>
    {
        /// <summary>
        /// Find button with Text in keyboard user
        /// </summary>
        /// <param name="text">Need element</param>
        /// <param name="menu">Menu</param>
        /// <param name="pages">Keyboard user</param>
        /// <returns>Button or null</returns>
        private KeyboardButton FindButton(string text, ReplyMarkupKeyboard keyboard)
        {
            if (keyboard == null) return null;
            if (keyboard.Menu?.Name == text) return keyboard.Menu;
            
            foreach (var page in keyboard.Pages)
            {
                foreach (var row in page.Rows)
                {
                    foreach (var button in row.Buttons)
                    {
                        if (button.Name == text) return button;
                        if (!(button.Action is DefaultButtonAction defaultButtonAction)) continue;
                        if (defaultButtonAction.Executor == text) return button;
                    }
                }
            }

            return null;
        }

        private KeyboardButton FindButton(string text, InlineMarkupKeyboard keyboard)
        {
            if (keyboard == null) return null;
            
            foreach (var page in keyboard.Pages)
            {
                foreach (var row in page.Rows)
                {
                    foreach (var button in row.Buttons)
                    {
                        if (button.Name == text) return button;
                        if (!(button.Action is DefaultButtonAction defaultButtonAction)) continue;
                        if (defaultButtonAction.Executor == text) return button;
                    }
                }
            }

            return null;
        }

        private KeyboardButton FindButton(string text, User user, out bool isInline)
        {
            var button = FindButton(text, user.GetChat().ReplyMarkupKeyboard);
            if (button != null)
            {
                isInline = false;
                return button;
            }
            
            isInline = true;
            return FindButton(text, user.GetChat().InlineMarkupKeyboard);
        }

        private void HandleReadMessageEvent(User user, MessageNewContent content, UserChat chat)
        {
            var btn = !user.GetChat().HasKeyboard() 
                ? null 
                : FindButton(content.Text, user, out var _);

            if (btn == null)
            {
                chat.NewMessageAction?.Invoke(content.Text);
                return;
            }

            if (!(btn.Action is DefaultButtonAction defaultButtonAction))
            {
                return; 
            }
            
            if (defaultButtonAction.Executor?.StartsWith("/page ") ?? false)
                defaultButtonAction.Action = _ =>
                {
                    SiteProvider.EmulateExecute(user, defaultButtonAction.Executor);
                };
            else if (btn.Action == user.GetChat().ReplyMarkupKeyboard.Menu.Action)
            {
                chat.NewMessageAction = null;
            }

            if (defaultButtonAction.Action != null)
            {
                var tmp = chat.NewMessageAction;
                chat.NewMessageAction = null;
                defaultButtonAction.Action.Invoke(user);
                chat.NewMessageAction = tmp;
                return;
            }

            chat.NewMessageAction?.Invoke(defaultButtonAction.Executor);
        }
        
        public override void Handle(User user, MessageNewContent content)
        {
            if (content.PeerId != (long) user.Id) return;
            
            user.LastPeerId = content.PeerId;
            var keyboardUser = content.Text?.StartsWith("from ") ?? false
                ? user.Provider.GetOrCreateUser(ulong.Parse(content.Text.Substring(5)))
                : user;

            var chat = keyboardUser.GetChat();

            
            if (chat.NewMessageAction != null)
            {
                HandleReadMessageEvent(user, content, chat);
                return;
            }

            var message = content.Text;
            if (content.Payload != null)
                message = "/" + JObject.Parse(content.Payload)["command"];
            
            var isFromKeyboard = false;

            if (chat.HasKeyboard())
            {
                var btn = FindButton(content.Text, keyboardUser, out var isInline);
                if (btn == null && chat.ReplyMarkupKeyboard != null)
                {
                    if (chat.ReplyMarkupKeyboard != null && chat.InlineMarkupKeyboard != null) 
                        user.Send(new Message(chat.KeyboardMessage, 
                            new ReplyMarkupKeyboard(chat.ReplyMarkupKeyboard), 
                            new InlineMarkupKeyboard(chat.InlineMarkupKeyboard)), user.LastPeerId);
                    else if (chat.ReplyMarkupKeyboard != null)
                        user.Send(new Message(chat.KeyboardMessage, 
                            new ReplyMarkupKeyboard(chat.ReplyMarkupKeyboard)), user.LastPeerId);
                    else 
                        user.Send(new Message(chat.KeyboardMessage, 
                            new InlineMarkupKeyboard(chat.InlineMarkupKeyboard)), user.LastPeerId);

                    return;
                }
               
                
                if (btn?.Action is DefaultButtonAction defaultButtonAction)
                {
                    var oldKeyboard = chat.ReplyMarkupKeyboard;
                
                    if (chat.ReplyMarkupKeyboard != null && oldKeyboard == chat.ReplyMarkupKeyboard 
                                                         && chat.ReplyMarkupKeyboard.IsOneTime && 
                                                         !(defaultButtonAction.Executor?.StartsWith("/page ") ?? false) && !isInline) chat.KeyboardReset();

                    if (!defaultButtonAction.Executor?.StartsWith("/page") ?? true) defaultButtonAction.Action?.Invoke(user);
                
                    if (defaultButtonAction.Executor == null) return;

                    message = defaultButtonAction.Executor;
                    isFromKeyboard = true;
                }
                else
                {
                    return;
                }
            }

            if (!message.StartsWith("/"))
            {
                if (content.PeerId == (long)user.Id) SiteProvider.EmulateExecute(user, "/error unknown_command");
                return;
            }
            
            message = message.Substring(1);

            var args = message.Split(' ').ToList();

            var command = args[0];
            args.RemoveAt(0);

            var commandExecutor = SiteProvider.BotInstance.GetCommandExecutor(command);
            var arrayArgs = args.ToArray();

            if (commandExecutor == null || commandExecutor.IsHidden && !isFromKeyboard)
            {
                if (message != "error unknown_command" && content.PeerId == (long)user.Id)
                    SiteProvider.EmulateExecute(user, "/error unknown_command");
                return;
            }
            
            if ((commandExecutor.Scope == CommandScope.PrivateChat && content.PeerId != (long)user.Id) ||
                (commandExecutor.Scope == CommandScope.PublicChat && content.PeerId == (long)user.Id)) return;

            commandExecutor.User = user;
            commandExecutor.Args = arrayArgs;
            commandExecutor.QueryData = content.QueryData;
                
            foreach (var middleware in commandExecutor.Middlewares)
            {
                var responseMiddleware = middleware(commandExecutor);
                
                if (!responseMiddleware)
                    return;
            }
                
            if (!commandExecutor.PreProcessData()) return;
                
            var response = commandExecutor.Execute();
            if (response == null) return;
            user.Send(response.Value, content.PeerId);
        }
    }
}