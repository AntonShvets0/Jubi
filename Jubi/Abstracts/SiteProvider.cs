using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Exceptions;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Updates;
using Jubi.Updates.Types;
using SimpleIni;

namespace Jubi.Abstracts
{
    /// <summary>
    /// This class need for "standardize" generic SiteProvider and add they in one List
    /// Dont inherit this. Inherit generic SiteProvider
    /// </summary>
    public abstract class SiteProvider
    {
        /// <summary>
        /// Contains Api current serivce
        /// </summary>
        public abstract IApiProvider Api { get; set; }
        
        /// <summary>
        /// Identifier service
        /// </summary>
        public abstract string Id { get; set; }

        /// <summary>
        /// Type of user. Need, because is not generic class. Type used for create user instance in CreateInstance() 
        /// </summary>
        private Type _user;

        /// <summary>
        /// Instance of main class library
        /// </summary>
        public Bot BotInstance;

        internal SiteProvider(Type user)
        {
            if (user == typeof(User))
                throw new InvalidCastException("Argument 2 in SiteProvider must be contain User class");

            _user = user;
        }
        
        /// <summary>
        /// Contains Action delegate handlers for others received events.
        /// </summary>
        protected Dictionary<Type, Action<User, IUpdateContent>> EventHandlers 
            = new Dictionary<Type, Action<User, IUpdateContent>>();

        /// <summary>
        /// Start listening updates service
        /// </summary>
        public void Run()
        {
            SetProviderRefToMethod();
            
            while (true)
            {
                foreach (var updateInfo in Api.Updates.Get())
                {
                    CallEvent(updateInfo);
                }
            }
        }

        /// <summary>
        /// Set provider reference to field Provider in IMethodApiProvider
        /// </summary>
        private void SetProviderRefToMethod()
        {
            foreach (var prop in Api
                .GetType()
                .GetProperties().Where(p => 
                    p.PropertyType.GetInterfaces().Contains(typeof(IMethodApiProvider))))
            {
                var method = prop.GetValue(Api) as IMethodApiProvider;
                if (method == null) continue;

                method.Provider = Api;
                method.OnInit();
            }
        }

        protected abstract void CallEvent(UpdateInfo updateInfo);
        
        protected User CreateUserInstance()
        {
            var user = Activator.CreateInstance(_user) as User;
            if (user != null) user.Provider = this;
            
            return user;
        }
        
        /// <summary>
        /// Call in loop Bot, which call Start() 
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Emulate sendind message from user
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="str">Message</param>
        public abstract void EmulateExecute(User user, string str);

        /// <summary>
        /// Creating and return user, or get user from list and return his.
        /// </summary>
        /// <param name="id">Id user</param>
        /// <returns>User instance</returns>
        public abstract User GetOrCreateUser(ulong id);
        
        /// <summary>
        /// Contains all users, who handled SiteProvider
        /// </summary>
        public readonly HashSet<User> Users = new HashSet<User>();
    }
    
    public abstract class SiteProvider<T> : SiteProvider 
        where T : User
    {
        // New, because old field has type User
        protected new Dictionary<Type, Action<T, IUpdateContent>> EventHandlers 
            = new Dictionary<Type, Action<T, IUpdateContent>>();
        
        /// <summary>
        /// This object need to lock() block, because Users get in others threads. This object sync them
        /// </summary>
        public object ThreadLockUsers = new object();
        
        protected SiteProvider() : base(typeof(T))
        {
            EventHandlers.Add(typeof(MessageNewContent), OnMessage);
        }

        /// <summary>
        /// Find button with Text in keyboard user
        /// </summary>
        /// <param name="text">Need element</param>
        /// <param name="menu">Menu</param>
        /// <param name="pages">Keyboard user</param>
        /// <returns>Button or null</returns>
        private KeyboardAction FindButton(string text, KeyboardAction menu, List<KeyboardPage> pages)
        {
            if (menu?.Name == text) return menu;
            
            foreach (var page in pages)
            {
                foreach (var row in page.Rows)
                {
                    foreach (var button in row.Buttons)
                    {
                        if (button.Name == text) return button;
                    }
                }
            }

            return null;
        }
        
        public override void EmulateExecute(User user, string str)
        {
            OnMessage(user as T, new MessageNewContent
            {
                Text = str
            });
        }

        /// <summary>
        /// When user send message
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="content">Update content</param>
        protected virtual void OnMessage(T user, IUpdateContent content)
        {
            var messageContent = content as MessageNewContent;
            var message = messageContent.Text;
            var isFromKeyboard = false;

            if (user.Keyboard?.Pages.Count != null)
            {
                var btn = FindButton(messageContent.Text, user.Keyboard.Menu, user.Keyboard.Pages);
                if (btn == null)
                {
                    user.Send(new Message(null,
                        new ReplyMarkupKeyboard(user.Keyboard)));
                    return;
                }


                if (user.Keyboard.IsOneTime)
                {
                    user.KeyboardReset();
                }

                btn.Action?.Invoke();

                if (btn.Executor == null) return;

                message = btn.Executor;
                isFromKeyboard = true;
            }

            if (!message.StartsWith("/")) return;
            message = message.Substring(1);

            var args = message.Split(' ').ToList();

            var command = args[0];
            args.RemoveAt(0);

            var commandExecutor = BotInstance.GetCommandExecutor(command);
            var arrayArgs = args.ToArray();

            if (commandExecutor == null || commandExecutor.IsHidden && !isFromKeyboard)
            {
                if (message != "error unknown_command")
                    EmulateExecute(user, "/error unknown_command");
                return;
            }

            try
            {
                commandExecutor.User = user;
                commandExecutor.Args = arrayArgs;
                
                foreach (var middleware in commandExecutor.Middlewares)
                {
                    var responseMiddleware = middleware(commandExecutor);
                    if (responseMiddleware == false)
                        return;
                }
                
                if (!commandExecutor.PreProcessData()) return;
                
                var response = commandExecutor.Execute();
                if (response == null) return;

                user.Send((Message) response);
            }
            catch (SyntaxErrorException ex)
            {
                user.Send(Error.FromConfig(BotInstance, "syntax") + 
                          $" /{commandExecutor.FullAlias} {ex.Message}");
            }
            catch (ErrorException ex)
            {
                user.Send(Error.FromConfig(BotInstance, "default") + $" {ex.Message}");
            }
        }

        /// <summary>
        /// Call event with UpdateInfo
        /// </summary>
        /// <param name="updateInfo">Update content</param>
        protected override void CallEvent(UpdateInfo updateInfo)
        {
            var content = updateInfo.UpdateContent;

            if (EventHandlers.ContainsKey(content.GetType())) {
                new Thread(() =>
                {
                    var user = GetOrCreateUser(updateInfo.Initiator);

                    if (content is MessageNewContent messageNew && user.IsWaitingResponse)
                    {
                        if (user.Keyboard != null)
                        {
                            var btn = FindButton(messageNew.Text, user.Keyboard.Menu, user.Keyboard.Pages);
                            if (btn == null)
                            {
                                user.ResponseLine = messageNew.Text;
                                return;
                            }

                            if (btn.Executor == "/page next" || btn.Executor == "/page previous")
                            {
                                btn.Action = () =>
                                {
                                    EmulateExecute(user, btn.Executor);
                                };
                            }

                            if (btn.Action != null)
                            {
                                btn.Action.Invoke();
                                return;
                            }

                            user.ResponseLine = btn.Executor;
                            return;
                        }
                        
                        user.ResponseLine = messageNew.Text;
                        return;
                    }
                    
                    lock (user.ThreadLockUser)
                    {
                        try
                        {
                            EventHandlers[content.GetType()]?.Invoke(user as T, content);
                        }
                        catch (Exception ex)
                        {
                            lock (Bot.LogsLock)
                                HandleError(ex, user as T);
                        }
                    }
                }).Start();
            }
        }

        /// <summary>
        /// Call, while command throw exception.
        /// Logging
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <param name="user">User</param>
        private void HandleError(Exception ex, T user)
        {
            if (Bot.Logs.ContainsKey(user))
            {
                Bot.Logs[user].Add(ex);
            }
            else
            {
                Bot.Logs.Add(user, new List<Exception>
                {
                    ex
                });
            }

            try
            {
                var keyboard = user.Keyboard;
                var page = user.KeyboardPage;
                
                user.KeyboardReset();

                EmulateExecute(user, "/error exception");
                Api.Messages.Send(Error.FromConfig(BotInstance, "internal_error"), user);

                if (user.Keyboard == null)
                {
                    user.Keyboard = keyboard;
                    user.KeyboardPage = page;
                }
            }
            catch (Exception) { }
        }
        
        public override User GetOrCreateUser(ulong id)
        {
            lock (ThreadLockUsers)
            {
                var user = Users.FirstOrDefault(u => u.Id == id);
                if (user != null) return user;

                user = CreateUserInstance();
                user.Id = id;
                Users.Add(user);

                return user;
            }
        }

        private new T CreateUserInstance()
        { 
            var user = Activator.CreateInstance<T>();
            user.Provider = this;
 
            return user;
        }

        protected string AccessToken;

        public override void OnInit()
        {
            if (BotInstance.Configuration["apiKeys"]?[Id] == null)
                throw new JubiException($"{Id} api key not found");
            
            AccessToken = BotInstance.Configuration["apiKeys"][Id];
        }
    }
}