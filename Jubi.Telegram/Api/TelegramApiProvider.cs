using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Telegram.Api.Types;
using Jubi.Telegram.Exceptions;
using Newtonsoft.Json.Linq;

namespace Jubi.Telegram.Api
{
    public class TelegramApiProvider : IApiProvider
    {
        public string AccessToken { get; set; }

        public IMessageApiProvider Messages { get; } = new TelegramMessageApiProvider();
        public IUpdateApiProvider Updates { get; } = new TelegramUpdateApiProvider();

        public TelegramChannelApiProvider Channels { get; } = new TelegramChannelApiProvider();

        public TelegramApiProvider(string token)
        {
            AccessToken = token;
        }

        public JToken SendRequest(string method, Dictionary<string, string> args, bool throwException = false)
        {
            var response =
                WebProvider.SendRequestAndGetJson($"https://api.telegram.org/bot{AccessToken}/{method}", args);

            if (!(bool) response["ok"])
            {
                if (throwException) throw new TelegramErrorException(
                    int.Parse(response["error_code"].ToString()), 
                    response["description"].ToString()
                );

                return null;
            }

            return response["result"];
        }

        public JToken SendMultipartRequest(string method, List<WebMultipartContent> args, bool throwException = false)
        {
            var response =
                WebProvider.SendMultipartRequestAndGetJson($"https://api.telegram.org/bot{AccessToken}/{method}", args);

            if (!(bool) response["ok"])
            {
                if (throwException) throw new TelegramErrorException(
                    int.Parse(response["error_code"].ToString()), 
                    response["description"].ToString()
                );

                return null;
            }

            return response["result"];
        }

        public JObject BuildKeyboard(KeyboardAction menu, KeyboardPage keyboard, bool isOneTime = false)
        {
            var buttons = new JArray();
            foreach (var row in keyboard.Rows)
            {
                buttons.Add(new JArray());
                
                foreach (var button in row.Buttons)
                {
                    (buttons[buttons.Count - 1] as JArray).Add(button.Name);
                }
            }

            if (menu != null)
            {
                buttons.Add(new JArray
                {
                    {menu.Name}
                });
            }
            
            return new JObject
            {
                {"one_time_keyboard", isOneTime},
                {"resize_keyboard", true},
                {"keyboard", buttons}
            };
        }
    }
}