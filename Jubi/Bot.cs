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
        public Dictionary<string, Type> CommandExecutors;
        public object CommandExecutorsLock = new object();

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
            else Configuration = Ini.FromFile(config);

            Providers = siteProviders.ToHashSet();

            ReplyMarkupKeyboard.PreviousButtonText = Configuration["buttons"]["previous"];
            ReplyMarkupKeyboard.NextButtonText = Configuration["buttons"]["next"];
            ReplyMarkupKeyboard.MenuText = Configuration["buttons"]["menu"];

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
                GetCommandExecutorsViaReflection(executorInformation.Assembly, executorInformation.Namespace);
            
            foreach (var command in 
                GetCommandExecutorsViaReflection(Assembly.GetExecutingAssembly(), "Jubi.Executors"))
            {
                if (CommandExecutors.ContainsKey(command.Key))
                    CommandExecutors[command.Key] = command.Value;
                else
                    CommandExecutors.Add(command.Key, command.Value);
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

        internal CommandExecutor GetCommandExecutor(Type type)
        {
            var obj = Activator.CreateInstance(type);
            var executor = obj as CommandExecutor;
            if (executor != null) executor.BotInstance = this;
            
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
                          "menu = Back to menu\n" +
                          "[apiKeys]\n" +
                          "; put here api keys for Jubi providers\n" +
                          "; field for api should be called like the provider with a lowercase letter" +
                          "(TelegramProvider -> telegram, VKontakteProvider -> vkontakte)\n" +
                          "exampleService = api_key\n" +
                          "[errors]\n" +
                          "default = Error:\n" +
                          "syntax = Syntax error! True syntax:\n" +
                          "int_convert = Error: You must write number!\n" +
                          "bool_convert = Error: You must write \"false\" or \"true\"!\n" +
                          "internal_error = Error: Internal error while executing command\n" +
                          "unknown_command = Error: Unknown command\n\n" +
                          "[types]\n" +
                          "System.Int32 = number\n" +
                          "System.Int64 = number\n" + 
                          "System.Decimal = number\n" + 
                          "System.Char = symbol\n" + 
                          "System.UInt32 = positive number\n" +
                          "System.UInt64 = positive number\n" + 
                          "System.Boolean = false/true\n" + 
                          "System.String = string";
            var ini = new Ini(content);
            
            File.WriteAllText(pathToFile, content);
            return ini;
        }
    }
}