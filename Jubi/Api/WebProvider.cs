using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Jubi.Api
{
    public class WebProvider
    {
        public static JObject SendRequestAndGetJson(string url, NameValueCollection args)
        {
            try
            {
                var client = new WebClient();
                return JObject.Parse(Encoding.UTF8.GetString(client.UploadValues(url, args)));
            }
            catch (WebException webException)
            {
                var stream = webException.Response?.GetResponseStream();
                if (stream == null) throw;

                var content = new StreamReader(stream).ReadToEnd();
                return JObject.Parse(content);
            }
        }

        public static JObject SendMultipartRequestAndGetJson(string url, IEnumerable<WebMultipartContent> args)
            => JObject.Parse(SendMultipartRequest(url, args));

        public static string SendMultipartRequest(string url, IEnumerable<WebMultipartContent> args)
        {
            var client = new HttpClient();
            var multipart = new MultipartFormDataContent();

            foreach (var arg in args)
            {
                multipart.Add(arg.Content, arg.Name, "test.jpg");
            }

            var task = client.PostAsync(url, multipart);
            return new StreamReader(task.Result.Content.ReadAsStreamAsync().Result).ReadToEnd();
        }
    }
}