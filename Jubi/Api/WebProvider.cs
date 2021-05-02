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
        private static HttpClient _httpClient = new HttpClient();
        public static JObject SendRequestAndGetJson(string url, Dictionary<string, string> args)
        {
            while (true)
            {
                try
                {
                    return JObject.Parse(
                        _httpClient.PostAsync(url, new FormUrlEncodedContent(args)).Result.Content.ReadAsStringAsync()
                            .Result
                    );
                }
                catch (WebException webException)
                {
                    var stream = webException.Response?.GetResponseStream();
                    if (stream == null) continue;

                    var content = new StreamReader(stream).ReadToEnd();
                    return JObject.Parse(content);
                }
                catch
                {
                    continue;
                }
            }
        }

        public static JObject SendMultipartRequestAndGetJson(string url, IEnumerable<WebMultipartContent> args)
            => JObject.Parse(SendMultipartRequest(url, args));

        public static string SendMultipartRequest(string url, IEnumerable<WebMultipartContent> args)
        {
            var multipart = new MultipartFormDataContent();

            foreach (var arg in args)
            {
                if (arg.Content is StringContent)
                    multipart.Add(arg.Content, arg.Name);
                else
                    multipart.Add(arg.Content, arg.Name, "image.jpg");
            }

            var task = _httpClient.PostAsync(url, multipart);
            return new StreamReader(task.Result.Content.ReadAsStreamAsync().Result).ReadToEnd();
        }
    }
}