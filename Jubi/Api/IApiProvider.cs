using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Newtonsoft.Json.Linq;

namespace Jubi.Api
{
    public interface IApiProvider
    {
        public string AccessToken { get; set; }
        
        public IMessageApiProvider Messages { get; }
        
        public IUpdateApiProvider Updates { get; }

        JToken SendRequest(string method, Dictionary<string, string> args, bool throwException = true);

        JObject BuildKeyboard(KeyboardAction menu, KeyboardPage keyboard, bool isOneTime = false);
    }
}