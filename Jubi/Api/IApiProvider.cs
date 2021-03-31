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

        JToken SendRequest(string method, NameValueCollection args);

        JObject BuildKeyboard(KeyboardPage keyboard, bool isOneTime = false);
    }
}