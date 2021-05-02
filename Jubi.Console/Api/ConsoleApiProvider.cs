using System;
using System.Collections.Generic;
using System.Threading;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Console.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Newtonsoft.Json.Linq;

namespace Jubi.Console.Api
{
    public class ConsoleApiProvider : IApiProvider
    {
        public string AccessToken { get; set; }
        public IMessageApiProvider Messages { get; } = new ConsoleMessageApiProvider();
        public IUpdateApiProvider Updates { get; } = new ConsoleUpdateApiProvider();

        public JToken SendRequest(string method, Dictionary<string, string> args, bool throwException = true)
        {
            Thread.Sleep(new Random().Next(50, 600)); // Эмуляция запроса
            return null;
        }

        public JObject BuildKeyboard(KeyboardAction menu, KeyboardPage keyboard, bool isOneTime = false)
        {
            return null;
        }
    }
}