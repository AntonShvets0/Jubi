using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
            var tgUser = user as TelegramUser;
            
            var attachments = new List<Tuple<string, string>>();
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
            
            var args = new Dictionary<string, string>
            {
                {"chat_id", user.Id.ToString()},
                {"reply_markup", keyboard}
            };
            
            if (attachments.Count == 0)
            {
                if (string.IsNullOrEmpty(response.Text))
                {
                    response.Text = "\u2062";
                } 
                
                args.Add("text", response.Text);
                
                return Provider.SendRequest("sendMessage", args, false) != null;
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
                        {"media", attachment.Item2}
                    };
                    
                    jArray.Add(obj);
                }
                
                args.Add("media", jArray.ToString());
                
                return Provider.SendRequest("sendMediaGroup", args, false) != null;
            }

            args.Add(media.ParameterText, response.Text);
            args.Add(attachments[0].Item1, attachments[0].Item2);
            
            return Provider.SendRequest($"send{media.Type}", args, false) != null;
        }


        private Tuple<string, string> HandleAttachment(User user, IAttachment attachment)
        {
            if (attachment is PhotoAttachment photoAttachment)
            {
                // TODO: загрузка по потоку байтов
                if (photoAttachment.Url != null)
                {
                    return new Tuple<string, string>("photo", photoAttachment.Url);
                }
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