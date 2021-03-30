using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Updates;
using Jubi.Updates.Types;
using Newtonsoft.Json.Linq;

namespace Jubi.Telegram.Api.Types
{
    public class TelegramUpdateApiProvider : IUpdateApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit() {}

        private ulong TimeStamp;

        public IEnumerable<UpdateInfo> Get()
        {
            var array = Provider.SendRequest("getUpdates", new NameValueCollection
            {
                {"offset", TimeStamp.ToString()}
            }) as JArray;

            foreach (var updateToken in array)
            {
                var updateObject = updateToken as JObject;
                TimeStamp = (ulong)updateObject["update_id"];
                TimeStamp++;

                if (!updateObject.ContainsKey("message")) continue;
                
                yield return new UpdateInfo
                {
                    Initiator = (ulong)updateObject["message"]["from"]["id"],
                    UpdateContent = new MessageNewContent
                    {
                        Text = updateObject["message"]["text"].ToString()
                    }
                };
            }
        }
    }
}