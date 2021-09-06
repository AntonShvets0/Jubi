using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response.Attachments.Keyboard.Parameters;
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

        public IKeyboardApiProvider Keyboard { get; } = new TelegramKeyboardApiProvider();

        public TelegramChannelApiProvider Channels { get; } = new TelegramChannelApiProvider();

        public SiteProvider Provider { get; set; }

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
    }
}