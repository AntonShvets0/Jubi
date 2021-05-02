using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Updates;
using Jubi.Updates.Types;
using Jubi.VKontakte.Models;
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
        
        private VKontakteLongPollResponse _longPollServer;
        private ulong _ts;

        public IEnumerable<UpdateInfo> Get()
        {
            JObject response;

            while (true)
            {
                if (_longPollServer == null)
                {
                    _longPollServer = (Provider as VKontakteApiProvider).Groups.GetLongPollServer();
                    _ts = _longPollServer.TimeStamp;
                }
                

                try
                {
                    response = WebProvider.SendRequestAndGetJson(
                        $"{_longPollServer.Server}?act=a_check&key={_longPollServer.Key}&wait=25&ts={_ts}",
                        new Dictionary<string, string>()
                    );
                    if (response.ContainsKey("failed"))
                    {
                        _longPollServer = null;
                        continue;
                    }
                    
                    _ts = ulong.Parse(response["ts"].ToString());
                    break;
                }
                catch
                {
                    _longPollServer = null;
                }
            }

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
                        var updateInfo = 
                            TypeHandlers[evObject["type"].ToString()].Invoke(evObject["object"] as JObject);
                        if (updateInfo == null) continue;
                        yield return updateInfo;
                    }
                }
            }
        }

        private UpdateInfo OnMessageNew(JObject jObject)
        {
            var fromId = (string) jObject["message"]["from_id"];
            var peerId = (string) jObject["message"]["peer_id"];
            
            if (fromId != peerId)
                return null;
            
            return new UpdateInfo
            {
                Initiator = (ulong) jObject["message"]["from_id"],
                UpdateContent = new MessageNewContent
                {
                    Text = jObject["message"]?["text"]?.ToString(),
                    Payload = jObject["message"]?["payload"]?.ToString()
                }
            };
        }
    }
}