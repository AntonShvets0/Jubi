using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Attributes;
using Jubi.Telegram.Updates;
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
            var array = Provider.SendRequest("getUpdates", new Dictionary<string, string>()
            {
                {"offset", TimeStamp.ToString()}
            }) as JArray;

            foreach (var updateToken in array)
            {
                var updateObject = updateToken as JObject;
                TimeStamp = (ulong)updateObject["update_id"];
                TimeStamp++;

                var types = new [] {"message", "edited_message", "callback_query"};

                foreach (var type in types)
                {
                    if (!updateObject.ContainsKey(type)) continue;

                    yield return Provider.Provider.CallEvent(type, updateObject);
                }
            }
        }

        [Event("message")]
        public UpdateInfo HandleMessage(JObject updateObject)
        {
            var peerId = updateObject["message"]?["chat"]?["id"];
            if (updateObject["message"]?["text"] == null) return null;
            if (updateObject["message"]?["from"]?["id"] == null) return null;
            
            return new UpdateInfo
            {
                Initiator = Provider.Provider.GetOrCreateUser((ulong)updateObject["message"]["from"]["id"]),
                UpdateContent = new MessageNewContent
                {
                    Text = updateObject["message"]?["text"]?.ToString(),
                    PeerId = peerId == null ? 0 : (long)peerId
                }
            };
        }

        [Event("callback_query")]
        public UpdateInfo HandleCallbackQuery(JObject updateObject)
        {
            var peer = updateObject["callback_query"]?["message"]?["chat"]?["id"];
            
            return new UpdateInfo
            {
                Initiator = Provider.Provider.GetOrCreateUser((ulong) updateObject["callback_query"]["from"]["id"]),
                UpdateContent = new CallbackQueryContent
                {
                    Data = updateObject["callback_query"]["data"].ToString(),
                    Id = updateObject["callback_query"]["id"].ToString(),
                    PeerId = peer == null ? 0 : (long)peer
                }
            };
        }
    }
}