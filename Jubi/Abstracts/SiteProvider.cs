using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Attributes;
using Jubi.EventHandlers;
using Jubi.Exceptions;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Updates;
using Jubi.Updates.Types;
using Newtonsoft.Json.Linq;
using SimpleIni;

namespace Jubi.Abstracts
{
    /// <summary>
    /// This class need for "standardize" generic SiteProvider and add they in one List
    /// Dont inherit this. Inherit generic SiteProvider
    /// </summary>
    public abstract class SiteProvider
    {
        public abstract EventHandler[] EventHandlers { get; }

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
        /// Start listening updates service
        /// </summary>
        public void Run()
        {
            SetProviderRefToMethod();
            
            while (!BotInstance.IsStoped)
            {
                try
                {
                    foreach (var updateInfo in Api.Updates.Get())
                    {
                        if (updateInfo == null) continue;
                        
                        
                        CallEvent(updateInfo);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
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
            
            var eventMethods = Api.Updates.GetType().GetMethods()
                .Where(a => a.GetCustomAttribute<EventAttribute>()?.Event != null);
            foreach (var eventMethod in eventMethods)
            {
                if (_events.ContainsKey(eventMethod.GetCustomAttribute<EventAttribute>().Event)) continue;
                
                _events.Add(eventMethod.GetCustomAttribute<EventAttribute>().Event, 
                    (EventDelegate)Delegate.CreateDelegate(typeof(EventDelegate), Api.Updates, eventMethod));
            }
        }

        private Dictionary<string, EventDelegate> _events = new Dictionary<string, EventDelegate>();
        
        public delegate UpdateInfo EventDelegate(JObject updateData);

        public UpdateInfo CallEvent(string type, JObject updateData)
        {
            if (!_events.ContainsKey(type)) return null;

            return _events[type](updateData);
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
        public abstract void EmulateExecute(User user, string str, long peerId = 0);

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
        protected string AccessToken;
        
        /// <summary>
        /// This object need to lock() block, because Users get in others threads. This object sync them
        /// </summary>
        public object ThreadLockUsers = new object();
        
        protected SiteProvider() : base(typeof(T))
        {
        }

        public override void EmulateExecute(User user, string str, long peerId = 0)
        {
            HandleEvent(EventHandlers.OfType<MessageEventHandler>().FirstOrDefault(), new UpdateInfo
            {
                Initiator = user,
                UpdateContent = new MessageNewContent
                {
                    Payload = null,
                    Text = str,
                    PeerId = peerId == 0 ? (long)user.Id : peerId
                }
            });
        }
        
        public override void OnInit()
        {
            if (BotInstance.Configuration["apiKeys"]?[Id] == null)
                throw new JubiException($"{Id} api key not found");
            
            AccessToken = BotInstance.Configuration["apiKeys"][Id];
            foreach (var eventHandler in EventHandlers)
            {
                eventHandler.SiteProvider = this;
            }
        }
        

        /// <summary>
        /// Call event with UpdateInfo
        /// </summary>
        /// <param name="updateInfo">Update content</param>
        protected override void CallEvent(UpdateInfo updateInfo)
        {
            var eventHandler = EventHandlers.FirstOrDefault(f => f.IsAvailable(updateInfo.UpdateContent));
            if (eventHandler == null) return;
            
            lock (updateInfo.Initiator.ThreadPoolLock)
            {
                if (!updateInfo.Initiator.IsExecuting && updateInfo.Initiator.ThreadPoolActions.Count > 0) updateInfo.Initiator.ThreadPoolActions.Clear();
                
                if (updateInfo.Initiator.IsExecuting || updateInfo.Initiator.ThreadPoolActions.Count > 0)
                {
                    updateInfo.Initiator.ThreadPoolActions.Add(() =>
                    {
                        ThreadPool.QueueUserWorkItem(state => HandleEvent(eventHandler, updateInfo));
                    });
                    return;
                }

                updateInfo.Initiator.IsExecuting = true;
                ThreadPool.QueueUserWorkItem(state =>
                {
                    HandleEvent(eventHandler, updateInfo);
                });
            }

        }

        private void HandleEvent(EventHandler eventHandler, UpdateInfo updateInfo)
        {
            if (eventHandler == null) return;
            var user = updateInfo.Initiator;
            var content = updateInfo.UpdateContent;
            
            user.IsExecuting = true;

            try
            {
                if (user.ThreadPoolActions.Count != 0) 
                    lock (user.ThreadPoolLock) user.ThreadPoolActions.RemoveAt(0);

                eventHandler.Handle(user as T, content);
            }
            catch (ErrorException ex)
            {
                user.Send(Error.FromConfig(BotInstance, "default") + $" {ex.Message}");
            }
            catch (SyntaxErrorException ex)
            {
                user.Send(Error.FromConfig(BotInstance, "syntax") +
                          $" /{ex.Alias} {ex.Message}");
            }
            catch (Exception ex)
            {
                if (BotInstance.IsDebug) throw;
                    
                if (!(updateInfo.UpdateContent is MessageNewContent {Text: "/error exception"}))
                {
                    lock (Bot.LogsLock)
                        HandleError(ex, user as T);
                }
            }
            finally
            {
                if (user.ThreadPoolActions.Count == 0) user.IsExecuting = false;
            }

            if (user.ThreadPoolActions.Count == 0) return;
            var action = user.ThreadPoolActions[0];
            ThreadPool.QueueUserWorkItem(state => action());
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
                var chat = user.GetChat();
                var keyboard = chat.ReplyMarkupKeyboard;
                var page = chat.KeyboardPage;
                chat.KeyboardReset();

                EmulateExecute(user, "/error exception");

                if (chat.ReplyMarkupKeyboard == null)
                {
                    chat.ReplyMarkupKeyboard = keyboard;
                    chat.KeyboardPage = page;
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
    }
}