using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Attachments.Keyboard.Models;
using Jubi.VKontakte.Api.Types;
using Jubi.VKontakte.Exceptions;
using Jubi.VKontakte.Models;
using Newtonsoft.Json.Linq;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.VKontakte.Api
{
    public class VKontakteApiProvider : IApiProvider
    {
        public string AccessToken { get; set; }
        public VKontakteGroupInfo GroupInfo;
        
        public IMessageApiProvider Messages { get; } = new VKontakteMessageApiProvider();
        public IUpdateApiProvider Updates { get; } = new VKontakteUpdateApiProvider();
        public VKontakteGroupApiProvider Groups { get; } = new VKontakteGroupApiProvider();
        public VKontaktePhotoApiProvider Photos { get; } = new VKontaktePhotoApiProvider();

        public VKontakteWidgetApiProvider Widget { get; } = new VKontakteWidgetApiProvider();

        public IKeyboardApiProvider Keyboard { get; } = new VKonakteKeyboardApiProvider();

        public VKontakteCoverApiProvider Cover { get; } = new VKontakteCoverApiProvider();

        public SiteProvider Provider { get; set; }

        public const string API_VERSION = "5.130";
        
        public VKontakteApiProvider(string token)
        {
            AccessToken = token;
        }

        public JToken SendRequest(string method, Dictionary<string, string> args, bool throwException = true)
        {
            if (!args.ContainsKey("access_token")) 
                args.Add("access_token", AccessToken);
            
            if (!args.ContainsKey("v")) 
                args.Add("v", API_VERSION);
            
            var response = WebProvider.SendRequestAndGetJson($"https://api.vk.com/method/{method}", args);

            if (response.ContainsKey("error"))
            {
                if (throwException)
                    throw new VKontakteErrorException(
                        int.Parse(response["error"]["error_code"].ToString()), 
                        response["error"]["error_msg"].ToString()
                    );

                return null;
            }

            return response["response"];
        }

        internal string[] GetPhotoWithAllResolution(string prefix, JObject array)
        {
            var covers = new List<string>();
            
            foreach (var token in array)
            {
                if (token.Key.StartsWith(prefix + "_"))
                {
                    covers.Add(token.Value.ToString());
                }
            }

            return covers.ToArray();
        }
        
        public JToken SendRequest(string group, string method, Dictionary<string, string> args)
            => SendRequest($"{group}.{method}", args);
    }
}