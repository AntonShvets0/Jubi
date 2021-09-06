using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Abstracts;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Newtonsoft.Json.Linq;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.Api
{
    public interface IApiProvider
    {
        public string AccessToken { get; set; }
        
        public IMessageApiProvider Messages { get; }
        
        public IUpdateApiProvider Updates { get; }
        
        public IKeyboardApiProvider Keyboard { get; }
        
        public SiteProvider Provider { get; set; }

        JToken SendRequest(string method, Dictionary<string, string> args, bool throwException = true);
    }
}