using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Jubi.VKontakte.Api.Types;
using Jubi.VKontakte.Exceptions;
using Jubi.VKontakte.Models;
using Newtonsoft.Json.Linq;

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
        

        public const string API_VERSION = "5.130";
        
        public VKontakteApiProvider(string token)
        {
            AccessToken = token;
        }

        public JToken SendRequest(string method, NameValueCollection args)
        {
            args.Add("access_token", AccessToken);
            args.Add("v", API_VERSION);

            var response = WebProvider.SendRequestAndGetJson($"https://api.vk.com/method/{method}", args);

            if (response.ContainsKey("error")) 
                throw new VKontakteErrorException(
                    int.Parse(response["error"]["error_code"].ToString()), 
                    response["error"]["error_msg"].ToString()
                );
            
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
        
        public JToken SendRequest(string group, string method, NameValueCollection args)
            => SendRequest($"{group}.{method}", args);

        public JObject BuildKeyboard(KeyboardPage keyboard, bool inline = false)
        {
            var buttons = new JArray();
            foreach (var row in keyboard.Rows)
            {
                buttons.Add(new JArray());
                
                foreach (var button in row.Buttons)
                {
                    (buttons[buttons.Count - 1] as JArray).Add(new JObject
                    {
                        {"action", new JObject
                        {
                            {"type", "text"},
                            {"label", button.Name},
                            {"payload", new JObject
                                {
                                    {"command", button.Executor}
                                }.ToString()
                            }
                        }},
                        {"color", button.Color switch
                        {
                            KeyboardColor.Green => "positive",
                            KeyboardColor.Red => "negative",
                            KeyboardColor.Primary => "primary",
                            _ => "secondary"
                        }}
                    });
                }
            }
            
            return new JObject
            {
                {"one_time", false},
                {"buttons", buttons},
                {"inline", false}
            };
        }
    }
}