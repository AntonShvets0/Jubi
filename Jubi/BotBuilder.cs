using System.IO;
using Jubi.Abstracts;
using Jubi.Models;
using SimpleIni;

namespace Jubi
{
    public class BotBuilder
    {
        public static Bot Create(string pathToConfiguration, SiteProvider[] providers,
            ExecutorInformation executorInformation)
        {
            Ini ini;
            
            if (!File.Exists(pathToConfiguration))
                ini = CreateDefaultIniConfiguration(pathToConfiguration);
            else
                ini = new Ini(File.ReadAllText(pathToConfiguration));

            return Create(ini, providers, executorInformation);
        }
        
        public static Bot Create(Ini configuration, SiteProvider[] providers, ExecutorInformation executorInformation)
        {
            return new Bot(configuration, providers, executorInformation);
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
                          "uint_convert = Error: You must write positive number!\n" +
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