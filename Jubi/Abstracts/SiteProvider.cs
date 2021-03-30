﻿using System;
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
        protected abstract IApiProvider Api { get; set; }
        
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
        public void Start()
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
            if (user != null) user.Api = Api;
            
            return user;
        }
        
        /// <summary>
        /// Call in loop Bot, which call Start() 
        /// </summary>
        public virtual void OnInit()
        {
        }
    }
    
    public abstract class SiteProvider<T> : SiteProvider 
        where T : User
    {
        // New, because old field has type User
        protected new Dictionary<Type, Action<T, IUpdateContent>> EventHandlers 
            = new Dictionary<Type, Action<T, IUpdateContent>>();
        
        /// <summary>
        /// Contains all users, who handled SiteProvider
        /// </summary>
        public HashSet<T> Users = new HashSet<T>();
        
        /// <summary>
        /// This object need to lock() block, because Users get in others threads. This object sync them
        /// </summary>
        public object ThreadLockUsers = new object();
        
        /// <summary>
        /// Identifier service
        /// </summary>
        public abstract string Id { get; set; }

        protected SiteProvider() : base(typeof(T))
        {
            EventHandlers.Add(typeof(MessageNewContent), OnMessage);
        }

        /// <summary>
        /// Find button with Text in keyboard user
        /// </summary>
        /// <param name="text">Need element</param>
        /// <param name="pages">Keyboard user</param>
        /// <returns>Button or null</returns>
        private KeyboardAction FindButton(string text, List<KeyboardPage> pages)
        {
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

        /// <summary>
        /// Emulate sendind message from user
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="str">Message</param>
        public void EmulateExecute(T user, string str)
        {
            OnMessage(user, new MessageNewContent
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

            if (user.Keyboard.Count != 0)
            {
                var btn = FindButton(messageContent.Text, user.Keyboard);
                btn?.Action?.Invoke();

                if (btn?.Executor == null) return;
                
                message = "/" + btn?.Executor;
                isFromKeyboard = true;
            }
            
            if (!message.StartsWith("/")) return;
            message = message.Substring(1);
            
            var args = message.Split(' ').ToList();
            
            var command = args[0];
            args.RemoveAt(0);

            var commandExecutor = 
                BotInstance.CommandExecutors.FirstOrDefault(c => c.Alias == command);
            var arrayArgs = args.ToArray();

            if (commandExecutor == null || (commandExecutor.IsHidden && !isFromKeyboard))
            {
                if (message != "error unknown_command") EmulateExecute(user, "/error unknown_command");
                return;
            }

            foreach (var middleware in commandExecutor.Middlewares)
            {
                var responseMiddleware = middleware(user, arrayArgs);
                if (responseMiddleware != null)
                {
                    user.Send((Message) responseMiddleware);
                    return;
                }
            }
            
            var response = commandExecutor.Execute(user, arrayArgs);
            if (response == null) return;

            user.Send((Message) response);
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
                        user.ResponseLine = messageNew.Text;
                        return;
                    }
                    
                    lock (user.ThreadLockUser)
                    {
                        try
                        {
                            EventHandlers[content.GetType()]?.Invoke(user, content);
                        }
                        catch (Exception ex)
                        {
                            lock (Bot.LogsLock)
                            {
                                HandleError(ex, user);
                            }
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
                EmulateExecute(user, "/error exception");
                Api.Messages.Send((string) BotInstance.Configuration["errors"]["exception"], user);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Creating and return user, or get user from list and return his.
        /// </summary>
        /// <param name="id">Id user</param>
        /// <returns>User instance</returns>
        private T GetOrCreateUser(ulong id)
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
            user.Api = Api;

            return user;
        }

        protected string AccessToken;

        public override void OnInit()
        {
            if (BotInstance.Configuration["apiKeys"]?["vkontakte"] == null)
                throw new JubiException($"{Id} api key not found");
            
            AccessToken = BotInstance.Configuration["apiKeys"][Id];
        }
    }
}