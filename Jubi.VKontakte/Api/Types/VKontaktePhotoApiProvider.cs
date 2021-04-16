using System.Collections.Generic;
using System.Collections.Specialized;
using Jubi.Api;
using Jubi.Api.Types;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontaktePhotoApiProvider : IMethodApiProvider
    {
        public IApiProvider Provider { get; set; }
        public void OnInit()
        {
            
        }

        public string GetMessagesUploadServer(ulong peerId)
        {
            return Provider.SendRequest("photos.getMessagesUploadServer", new Dictionary<string, string>
            {
                {"peer_id", peerId.ToString()}
            })["upload_url"]?.ToString();
        }

        public string SaveMessagesPhoto(string photo, string server, string hash)
        {
            var jObject = Provider.SendRequest("photos.saveMessagesPhoto", new Dictionary<string, string>
            {
                {"server", server},
                {"photo", photo},
                {"hash", hash}
            }) as JArray;

            return $"photo{jObject[0]["owner_id"]}_{jObject[0]["id"]}";
        }
    }
}