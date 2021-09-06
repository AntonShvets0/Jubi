using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Attributes;
using Jubi.Updates;
using Jubi.Updates.Types;
using Jubi.VKontakte.Enums;
using Jubi.VKontakte.Models;
using Jubi.VKontakte.Updates;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontakteUpdateApiProvider : IUpdateApiProvider
    {
        public IApiProvider Provider { get; set; }

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

                    if (!evObject.ContainsKey("type")) continue;

                    yield return Provider.Provider.CallEvent(evObject["type"].ToString(),
                        evObject["object"] as JObject);
                }
            }
        }

        [Event("message_new")]
        public UpdateInfo HandleMessage(JObject jObject)
        {
            var peerId = (long) jObject["message"]["peer_id"];
            
            return new UpdateInfo
            {
                Initiator = Provider.Provider.GetOrCreateUser((ulong)jObject["message"]["from_id"]),
                UpdateContent = new MessageNewContent
                {
                    Text = jObject["message"]?["text"]?.ToString(),
                    Payload = jObject["message"]?["payload"]?.ToString(),
                    QueryData = jObject["message"]?["ref"]?.ToString(),
                    PeerId = peerId
                }
            };
        }

        [Event("like_add")]
        public UpdateInfo HandleLikeAdd(JObject jObject)
        {
            return new UpdateInfo
            {
                Initiator = Provider.Provider.GetOrCreateUser((ulong) jObject["liker_id"]),
                UpdateContent = ParseLikeContent(jObject, LikeType.Like)
            };
        }
        
        [Event("like_remove")]
        public UpdateInfo HandleLikeRemove(JObject jObject)
        {
            return new UpdateInfo
            {
                Initiator = Provider.Provider.GetOrCreateUser((ulong) jObject["liker_id"]),
                UpdateContent = ParseLikeContent(jObject, LikeType.Dislike)
            };
        }

        private LikeContent ParseLikeContent(JObject jObject, LikeType type) => new LikeContent()
        {
            ObjectId = (int) jObject["object_id"],
            PostId = (int) jObject["post_id"],
            ObjectType = GetType(jObject["object_type"].ToString()),
            ObjectOwnerId = (long) jObject["object_owner_id"],
            Type = type
        };

        private VKontakteObjectType GetType(string type) => type switch
        {
            "market" => VKontakteObjectType.Market,
            "note" => VKontakteObjectType.Note,
            "photo" => VKontakteObjectType.Photo,
            "video" => VKontakteObjectType.Video,
            "video_comment" => VKontakteObjectType.VideoComment,
            "photo_comment" => VKontakteObjectType.PhotoComment,
            "topic_comment" => VKontakteObjectType.TopicComment,
            "market_comment" => VKontakteObjectType.MarketComment,
            "post" => VKontakteObjectType.Post,
            _ => VKontakteObjectType.Comment
        };
    }
}