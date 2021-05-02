using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response;
using Jubi.Response.Attachments;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;
using Jubi.Telegram.Models;
using Newtonsoft.Json.Linq;

namespace Jubi.Telegram.Api.Types
{
    public class TelegramMessageApiProvider : IMessageApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit() { }

        public bool Send(Message response, User user)
        {            
            var attachments = new List<Tuple<string, object>>();
            string keyboard = null;
            Type typeAttachment = null;

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
                            {"remove_keyboard", true}
                        }.ToString();
                        
                        user.KeyboardReset();
                        continue;
                    }
                    
                    if (typeAttachment != null && attachment.GetType() != typeAttachment) 
                        throw new InvalidCastException("Attachment is must be only one type");

                    typeAttachment = attachment.GetType();
                    
                    var attach = HandleAttachment(user, attachment);
                    if (attach == null) continue;
                    
                    attachments.Add(attach);
                }      
            }

            if (keyboard == null && user.Keyboard?.Pages?.Count != null)
            {
                keyboard = Provider.BuildKeyboard(
                    user.Keyboard.Menu, 
                    user.Keyboard.Pages[user.KeyboardPage]).ToString();
            }
            
            var args = new List<WebMultipartContent>()
            {
                new WebMultipartContent("chat_id", new StringContent(user.Id.ToString()))
            };

            if (keyboard != null)
                args.Add(new WebMultipartContent("reply_markup", new StringContent(keyboard)));

            if (attachments.Count == 0)
            {
                if (string.IsNullOrEmpty(response.Text))
                {
                    response.Text = "\u2062";
                } 
                
                args.Add(new WebMultipartContent("text", new StringContent(response.Text)));
                
                return (Provider as TelegramApiProvider).SendMultipartRequest("sendMessage", args, false) != null;
            }

            var media = GetMediaGroup(typeAttachment);
            if (attachments.Count >= 2)
            {
                var jArray = new JArray();
                foreach (var attachment in attachments)
                {
                    var obj = new JObject
                    {
                        {"type", media.Type.ToLower()},
                        {media.ParameterText, response.Text},
                    };

                    if (attachment.Item2 is byte[] bytes)
                        obj.Add("media", bytes);
                    else
                        obj.Add("media", attachment.Item2.ToString());

                    jArray.Add(obj);
                }
                
                args.Add(new WebMultipartContent("media", new StringContent(jArray.ToString())));
                
                return (Provider as TelegramApiProvider).SendMultipartRequest("sendMediaGroup", args, false) != null;
            }

            if (!string.IsNullOrEmpty(response.Text))
                args.Add(new WebMultipartContent(media.ParameterText, new StringContent(response.Text)));
            
            if (attachments[0].Item2 is byte[] bytesAttachment)
                args.Add(new WebMultipartContent(
                    attachments[0].Item1, new StreamContent(new MemoryStream(bytesAttachment))));
            else
                args.Add(new WebMultipartContent(
                    attachments[0].Item1, new StringContent(attachments[0].Item2.ToString())));                

            return (Provider as TelegramApiProvider).SendMultipartRequest($"send{media.Type}", args, false) != null;
        }


        private Tuple<string, object> HandleAttachment(User user, IAttachment attachment)
        {
            if (attachment is PhotoAttachment photoAttachment)
            {
                if (photoAttachment.Url != null 
                    && (photoAttachment.Url.StartsWith("https://") || photoAttachment.Url.StartsWith("http://")))
                {
                    return new Tuple<string, object>("photo", photoAttachment.Url);
                }
                
                return new Tuple<string, object>("photo", photoAttachment.Content ?? File.ReadAllBytes(photoAttachment.Url));
            }

            return null;
        }

        private TelegramMediaGroup GetMediaGroup(Type type)
        {
            if (type == typeof(PhotoAttachment))
                return new TelegramMediaGroup
                {
                    Type = "Photo",
                    ParameterText = GetTextParameter(type)
                };

            return null;
        }

        private string GetTextParameter(Type type)
        {
            if (type == typeof(PhotoAttachment)) return "caption";

            return "text";
        }
    }
}