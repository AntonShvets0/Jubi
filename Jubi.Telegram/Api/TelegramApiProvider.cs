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

        public TelegramApiProvider(string token)
        {
            AccessToken = token;
        }

        public JToken SendRequest(string method, NameValueCollection args)
        {
            var response =
                WebProvider.SendRequestAndGetJson($"https://api.telegram.org/bot{AccessToken}/{method}", args);
            
            if (!((bool)response["ok"]))
                throw new TelegramErrorException(
                    int.Parse(response["error_code"].ToString()), 
                    response["description"].ToString()
                );
            
            return response["result"];
        }

        public JObject BuildKeyboard(KeyboardPage keyboard, bool inline = false)
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
            
            return new JObject
            {
                {"one_time_keyboard", false},
                {"resize_keyboard", true},
                {"keyboard", buttons}
            };
        }
    }
}