using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Jubi.Abstracts;
using Jubi.Exceptions;
using Jubi.Models;
using Jubi.Response.Attachments.Keyboard;
using SimpleIni;

namespace Jubi
{
    public class Bot
    {
        public readonly Ini Configuration;
        
        private readonly HashSet<SiteProvider> Providers;
        internal HashSet<CommandExecutor> CommandExecutors;
        internal object CommandExecutorsLock = new object();

        /// <summary>
        /// Logs. Contains exception, which throwed in Executors.
        /// </summary>
        public static readonly Dictionary<User, List<Exception>> Logs = new Dictionary<User, List<Exception>>();
        
        /// <summary>
        /// Need for sync threads for Logs collection. 
        /// </summary>
        public static readonly object LogsLock = new object();
        
        public Bot(string config, IEnumerable<SiteProvider> siteProviders, ExecutorInformation executorInformation)
        {
            if (!File.Exists(config)) Configuration = CreateDefaultIniConfiguration(config);
            else Configuration = Ini.FromFile(File.ReadAllText(config));

            Providers = siteProviders.ToHashSet();

            ReplyMarkupKeyboard.PreviousButtonText = Configuration["buttons"]["previous"];
            ReplyMarkupKeyboard.NextButtonText = Configuration["buttons"]["next"];
            
            LoadExecutors(executorInformation);
        }

        /// <summary>
        /// Start all providers
        /// </summary>
        public void Start()
        {
            foreach (var provider in Providers)
            {
                provider.BotInstance = this;
                provider.OnInit();

                new Thread(() =>
                {
                    provider.Start();
                }).Start();
            }
        }

        /// <summary>
        /// Load CommandExecutors using reflection from custom namespace and Jubi.Executors
        /// </summary>
        private void LoadExecutors(ExecutorInformation executorInformation)
        {
            CommandExecutors =
                GetCommandExecutorsViaReflection(executorInformation.Assembly, executorInformation.Namespace).ToHashSet();
            
            foreach (var command in 
                GetCommandExecutorsViaReflection(Assembly.GetExecutingAssembly(), "Jubi.Executors"))
            {
                CommandExecutors.Add(command);
            }

            if (CommandExecutors.All(c => c.Alias != "error"))
            {
                throw new JubiException("Executor for handling errors not found (create executor with alias \"error\")");
            }
        }

        private IEnumerable<CommandExecutor> GetCommandExecutorsViaReflection(Assembly assembly, string ns)
        {
            return assembly.GetTypes().Where(
                    t => t.IsSubclassOf(typeof(CommandExecutor))
                         && !t.IsAbstract
                         &&
                         (t.Namespace?.StartsWith(ns) ?? false)
                )
                .Select(GetCommandExecutor);
        }

        private CommandExecutor GetCommandExecutor(object obj)
        {
            var executor = obj as CommandExecutor;
            executor.BotInstance = this;
            
            return executor;
        }

        /// <summary>
        /// Create default ini configuration for bot, and return Ini class
        /// </summary>
        /// <param name="pathToFile">Path to future configuration file</param>
        /// <returns>Ini</returns>
        public static Ini CreateDefaultIniConfiguration(string pathToFile)
        {
            var content = "[buttons]\n" +
                          "previous = Previous page\n" +
                          "next = Next page\n" +
                          "[apiKeys]\n" +
                          "; put here api keys for Jubi providers\n" +
                          "; field for api should be called like the provider with a lowercase letter " +
                          "(TelegramProvider -> telegram, VKontakteProvider -> vkontakte)\n" +
                          "exampleService = api_key\n" +
                          "[prefix_error]\n" +
                          "default = Error:\n" +
                          "syntax = Syntax error! True syntax:";
            var ini = new Ini(content);
            
            File.WriteAllText(pathToFile, content);
            return ini;
        }
    }
}