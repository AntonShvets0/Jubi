using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Jubi.Abstracts;
using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Exceptions;
using Jubi.Models;
using Jubi.Response.Attachments.Keyboard;
using SimpleIni;

namespace Jubi
{
    public class Bot
    {
        public readonly Ini Configuration;
        
        public readonly HashSet<SiteProvider> Providers;
        private Dictionary<string, Type> _commandExecutors;
        private object _commandExecutorsLock = new object();

        public bool IsThrowExceptions { get; set; } = true;

        /// <summary>
        /// Logs. Contains exception, which throwed in Executors.
        /// </summary>
        public static readonly Dictionary<User, List<Exception>> Logs = new Dictionary<User, List<Exception>>();
        
        /// <summary>
        /// Need for sync threads for Logs collection. 
        /// </summary>
        public static readonly object LogsLock = new object();
        
        internal Bot(Ini config, IEnumerable<SiteProvider> siteProviders, ExecutorInformation executorInformation)
        {
            Configuration = config;
            Providers = siteProviders.ToHashSet();

            ReplyMarkupKeyboard.PreviousButtonText = Configuration["buttons"]["previous"];
            ReplyMarkupKeyboard.NextButtonText = Configuration["buttons"]["next"];
            ReplyMarkupKeyboard.MenuText = Configuration["buttons"]["menu"];

            LoadExecutors(executorInformation);
        }

        /// <summary>
        /// Start all providers
        /// </summary>
        public void Run()
        {
            IsStoped = false;
            foreach (var provider in Providers)
            {
                provider.BotInstance = this;
                provider.OnInit();
                provider.Api.Provider = provider;

                new Thread(() =>
                {
                    provider.Run();
                }).Start();
            }
        }
        
        public bool IsStoped { get; private set; }
        
        public void Stop()
        {
            IsStoped = true;
        }

        /// <summary>
        /// Load CommandExecutors using reflection from custom namespace and Jubi.Executors
        /// </summary>
        private void LoadExecutors(ExecutorInformation executorInformation)
        {
            lock (_commandExecutorsLock)
            {
                _commandExecutors =
                    GetCommandExecutorsViaReflection(Assembly.GetExecutingAssembly(), "Jubi.Executors");
            
                foreach (var command in 
                    GetCommandExecutorsViaReflection(executorInformation.Assembly, executorInformation.Namespace))
                {
                    if (_commandExecutors.ContainsKey(command.Key))
                        _commandExecutors[command.Key] = command.Value;
                    else
                        _commandExecutors.Add(command.Key, command.Value);
                }
            }
        }

        private Dictionary<string, Type> GetCommandExecutorsViaReflection(Assembly assembly, string ns)
        {
            return assembly.GetTypes().Where(
                    t =>
                        t.IsSubclassOf(typeof(CommandExecutor))
                        && !t.IsAbstract
                        && t.IsClass
                        && (t.Namespace?.StartsWith(ns) ?? false)
                        && t.GetCustomAttribute<IgnoreAttribute>() == null
                ).Select(type
                    => new KeyValuePair<string, Type>(
                        type.GetCustomAttribute<CommandAttribute>()?.Alias ?? 
                            throw new InvalidOperationException("You haven't used the CommandAttribute attribute for the CommandExecutor"), 
                        type))
                .ToDictionary(k => k.Key, v => v.Value);
            
        }

        public CommandExecutor GetCommandExecutor(string alias)
        {
            lock (_commandExecutorsLock)
            {
                if (!_commandExecutors.ContainsKey(alias)) return null;
                var obj = Activator.CreateInstance(_commandExecutors[alias]);
                if (!(obj is CommandExecutor)) return null;
                
                var executor = obj as CommandExecutor;
                if (executor != null) executor.BotInstance = this;
            
                return executor;
            }
        }
    }
}