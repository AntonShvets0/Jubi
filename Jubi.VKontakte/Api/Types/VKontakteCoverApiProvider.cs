using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.VKontakte.Exceptions;
using Jubi.VKontakte.Models;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontakteCoverApiProvider : IMethodApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit() { }

        public string GetUploadServer(Crop crop1, Crop crop2)
        {
            var id = (Provider as VKontakteApiProvider)?.GroupInfo?.Id.ToString();

            return Provider.SendRequest("photos.getOwnerCoverPhotoUploadServer", new Dictionary<string, string>
            {
                {"crop_x", crop1.X.ToString()},
                {"crop_y", crop1.Y.ToString()},
                {"crop_x2", crop2.X.ToString()},
                {"crop_y2", crop2.Y.ToString()},
                {"group_id", id}
            })["upload_url"].ToString();
        }

        public bool SetCover(string hash, string photo)
        {
            return (Provider.SendRequest("photos.saveOwnerCoverPhoto", new Dictionary<string, string>
            {
                {"hash", hash},
                {"photo", photo}
            }, false) as JObject).ContainsKey("images");
        }

        public bool SetCover(string file) => SetCover(file, new Crop(0, 0), new Crop(795, 265));

        public bool SetCover(byte[] bytes) => SetCover(bytes, new Crop(0, 0), new Crop(795, 265));

        public bool SetCover(string file, Crop crop1, Crop crop2)
            => SetCover((Provider.Messages as VKontakteMessageApiProvider).GetBytes(file), crop1, crop2);

        public bool SetCover(byte[] bytes, Crop crop1, Crop crop2)
        {
            var jObject = WebProvider.SendMultipartRequestAndGetJson(GetUploadServer(crop1, crop2), new[]
            {
                new WebMultipartContent(
                    "photo",
                    new StreamContent(
                        new MemoryStream(bytes))) 
            });
            
            if (jObject.ContainsKey("error")) {
                throw new VKontakteErrorException(
                    int.Parse(jObject["error"]["error_code"].ToString()), 
                    jObject["error"]["error_msg"].ToString()
                );
            }
            
            return 
                jObject.ContainsKey("hash") && 
                SetCover(jObject["hash"].ToString(), jObject["photo"].ToString());
        }
    }
}