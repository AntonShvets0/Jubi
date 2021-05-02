using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.VKontakte.Models;
using Jubi.VKontakte.Models.Enums;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontakteGroupApiProvider : IMethodApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit()
        {
            (Provider as VKontakteApiProvider).GroupInfo = GetSelfInfo();
        }

        public VKontakteLongPollResponse GetLongPollServer()
        {
            var response = Provider.SendRequest("groups.getLongPollServer", new Dictionary<string, string> 
            {
                {"group_id", (Provider as VKontakteApiProvider)?.GroupInfo?.Id.ToString()}
            });
            
            return new VKontakteLongPollResponse
            {
                TimeStamp = ulong.Parse(response["ts"]?.ToString() ?? "0"),
                Key = response["key"]?.ToString(),
                Server = response["server"]?.ToString()
            };
        }
        
        public bool IsMember(User user)
        {
            return Provider.SendRequest("groups.isMember", new Dictionary<string, string>
            {
                {"group_id", (Provider as VKontakteApiProvider)?.GroupInfo?.Id.ToString()},
                {"user_id", user.Id.ToString()}
            })?.ToString() == "1";
        }
        
        

        public IEnumerable<VKontakteGroupInfo> GetById(params string[] ids)
        {
            if (ids.Length == 1 && ids[0] == "0") ids = Array.Empty<string>();

            var array = Provider.SendRequest("groups.getById",
                new Dictionary<string, string>
                    {{"group_ids", string.Join(",", ids)}}) as JArray;
            if (array == null) yield break;
            
            foreach (var jsonArrayElement in array)
            {
                var jsonElement = jsonArrayElement as JObject;
                if (jsonElement == null) continue;

                var visibility = jsonElement["is_closed"]?.ToString();
                var deactivated = jsonElement["deactivated"]?.ToString();
                var type = jsonElement["type"]?.ToString();

                yield return new VKontakteGroupInfo
                {
                    Id = (ulong) jsonElement["id"],
                    Cover = 
                        (Provider as VKontakteApiProvider).GetPhotoWithAllResolution("photo", jsonElement),
                    Name = jsonElement["name"]?.ToString(),
                    ScreenName = jsonElement["screen_name"]?.ToString(),
                    VisibilityGroupType = 
                        visibility switch
                        {
                            "0" => VisibilityGroupType.Open,
                            "1" => VisibilityGroupType.Closed,
                            _ => VisibilityGroupType.Private
                        },
                    DeactivatedType = 
                        deactivated switch
                        {
                            "deleted" => DeactivatedType.Deleted,
                            "banned" => DeactivatedType.Banned,
                            _ => DeactivatedType.Existing
                        },
                    GroupType = 
                        type switch
                        {
                            "page" => GroupType.Page,
                            "group" => GroupType.Group,
                            _ => GroupType.Event
                        }
                };
            }
        }

        public IEnumerable<VKontakteGroupInfo> GetById(params ulong[] ids) =>
            GetById(ids.Select(id => id.ToString()).ToArray());

        public VKontakteGroupInfo GetSelfInfo() => GetById(0).FirstOrDefault();
    }
}