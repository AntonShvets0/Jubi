using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

        public bool Delete(int messageId, User user)
        {
            return Provider.SendRequest("messages.delete", new Dictionary<string, string>
            {
                {"message_ids", messageId.ToString()},
                {"delete_for_all", "1"}
            }, false) != null;
        }

        public int Send(Message response, User user, long peerId = 0)
        {
            var attachments = new List<string>();
            string keyboard = null;

            if (string.IsNullOrEmpty(response.Text))
            {
                response.Text = "&#4448;";
            }
            
            var chat = user.GetChat(peerId);

            if (response.Attachments != null)
            {
                if (response.Attachments.OfType<AbstractKeyboard>().Count() == 2)
                {
                    var inline = response.Attachments.First(a => a is ReplyMarkupKeyboard) as ReplyMarkupKeyboard;
                    var id = Send(new Message(null, inline), user, peerId);
                    if (id != 0)
                        Delete(id, user);

                    var tmpAttachments = response.Attachments.ToList();
                    tmpAttachments.Remove(inline);
                    response.Attachments = tmpAttachments.ToArray();
                }

                foreach (var attachment in response.Attachments)
                {
                    if (attachment is ReplyMarkupKeyboard markupKeyboard)
                    {
                        if (!markupKeyboard.IsEmpty) 
                        {
                            keyboard = markupKeyboard.ToString(user, response.Text, Provider);
                            continue;
                        }
                        
                        keyboard = new JObject
                        {
                            {"buttons", new JArray()},
                            {"one_time", true}
                        }.ToString();

                        chat.KeyboardReset();
                        continue;
                    }

                    if (attachment is InlineMarkupKeyboard inlineMarkupKeyboard)
                    {
                        keyboard = inlineMarkupKeyboard.ToString(user, response.Text, Provider);
                        continue;
                    }
                    
                    var attach = HandleAttachment(user, attachment);
                    if (attach == null) continue;
                    
                    attachments.Add(attach);
                }
            }

            if (keyboard == null && chat.ReplyMarkupKeyboard?.Pages?.Count != null)
            {
                keyboard = Provider.Keyboard.
                    BuildReplyMarkupKeyboard(chat.ReplyMarkupKeyboard.Menu, chat.ReplyMarkupKeyboard.Pages[chat.KeyboardPage]).ToString();
            }

            if (peerId == 0) peerId = (long)user.Id;
            var request = Provider.SendRequest("messages.send", new Dictionary<string, string>
            {
                {"peer_id", peerId.ToString()},
                {"message", response.Text},
                {"keyboard", keyboard},
                {"random_id", new Random().Next(1, 10000).ToString()},
                {"attachment", string.Join(",", attachments)}
            }, false);
            if (request == null) return 0;

            return (int) request;
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