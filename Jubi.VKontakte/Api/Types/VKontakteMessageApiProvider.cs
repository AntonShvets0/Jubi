using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response;
using Jubi.Response.Attachments;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontakteMessageApiProvider : IMessageApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit()
        {
            
        }

        public bool Send(Message response, User user)
        {
            var attachments = new List<string>();
            string keyboard = null;

            if (string.IsNullOrEmpty(response.Text))
            {
                response.Text = "&#4448;";
            }
            
            if (response.Attachments != null)
            {
                foreach (var attachment in response.Attachments)
                {
                    if (attachment is ReplyMarkupKeyboard markupKeyboard)
                    {
                        if (!markupKeyboard.IsEmpty) 
                        {
                            keyboard = markupKeyboard.ToString(user, Provider);
                            continue;
                        }
                        
                        keyboard = new JObject
                        {
                            {"buttons", new JArray()},
                            {"one_time", true}
                        }.ToString();

                        user.KeyboardReset();
                        continue;
                    }
                    
                    var attach = HandleAttachment(user, attachment);
                    if (attach == null) continue;
                    
                    attachments.Add(attach);
                }      
            }

            if (keyboard == null && user.Keyboard?.Pages?.Count != null)
            {
                keyboard = Provider.BuildKeyboard(user.Keyboard.Menu, user.Keyboard.Pages[user.KeyboardPage]).ToString();
            }

            return Provider.SendRequest("messages.send", new Dictionary<string, string>
            {
                {"user_id", user.Id.ToString()},
                {"message", response.Text},
                {"keyboard", keyboard},
                {"random_id", new Random().Next(1, 10000).ToString()},
                {"attachment", string.Join(",", attachments)}
            }, false) != null;
        }

        private string HandleAttachment(User user, IAttachment attachment)
        {
            var vkProvider = Provider as VKontakteApiProvider;

            if (attachment is PhotoAttachment photo)
            {
                var url = vkProvider.Photos.GetMessagesUploadServer(user.Id);
                var jObject = WebProvider.SendMultipartRequestAndGetJson(url, new[]
                {
                    new WebMultipartContent(
                        "photo",
                        new StreamContent(
                            new MemoryStream(photo.Content ?? GetBytes(photo.Url)))) 
                });

                return vkProvider.Photos.SaveMessagesPhoto(jObject["photo"].ToString(), jObject["server"].ToString(),
                    jObject["hash"].ToString());
            }

            return "";
        }

        private byte[] GetBytes(string file)
        {
            if (file.StartsWith("http://") || file.StartsWith("https://"))
            {
                return new WebClient().DownloadData(file);
            }

            return File.ReadAllBytes(file);
        }
    }
}