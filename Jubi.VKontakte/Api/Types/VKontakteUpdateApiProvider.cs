using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Updates;
using Jubi.Updates.Types;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontakteUpdateApiProvider : IUpdateApiProvider
    {
        public IApiProvider Provider { get; set; }
        private Dictionary<string, Func<JObject, UpdateInfo>> TypeHandlers 
            = new Dictionary<string, Func<JObject, UpdateInfo>>();

        public VKontakteUpdateApiProvider()
        {
            TypeHandlers.Add("message_new", OnMessageNew);
        }

        public void OnInit()
        {
            
        }

        public IEnumerable<UpdateInfo> Get()
        {
            var longPollServer = (Provider as VKontakteApiProvider).Groups.GetLongPollServer();
            var response = WebProvider.SendRequestAndGetJson(
                $"{longPollServer.Server}?act=a_check&key={longPollServer.Key}&ts={longPollServer.TimeStamp}&wait=25", 
                new NameValueCollection()
                );
            
            foreach (var eventKeyValuePair in response)
            {
                if (!(eventKeyValuePair.Value is JArray)) continue;
                var array = eventKeyValuePair.Value as JArray;
                foreach (var ev in array)
                {
                    var evObject = ev as JObject;
                    if (evObject == null) continue;
                    
                    if (evObject.ContainsKey("type") && TypeHandlers.ContainsKey(evObject["type"].ToString()))
                    {
                        yield return TypeHandlers[evObject["type"].ToString()].Invoke(evObject["object"] as JObject);
                    }
                }
            }
        }

        private UpdateInfo OnMessageNew(JObject jObject)
        {
            return new UpdateInfo
            {
                Initiator = (ulong) jObject["message"]["from_id"],
                UpdateContent = new MessageNewContent
                {
                    Text = jObject["message"]["text"]?.ToString()
                }
            };
        }
    }
}