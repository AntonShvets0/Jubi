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

        public int Send(Message response, User user, long peerId = 0) => Send(response, user, peerId, "HTML");

        public int Send(Message response, User user, long peerId, string parseMode)
        {            
            var attachments = new List<Tuple<string, object>>();
            string keyboard = null;
            Type typeAttachment = null;

            var chat = user.GetChat(peerId);
            if (response.Attachments != null)
            {
                if (response.Attachments.OfType<AbstractKeyboard>().Count() == 2)
                {
                    var inline = response.Attachments.First(a => a is ReplyMarkupKeyboard) as ReplyMarkupKeyboard;
                    var id = Send(new Message(null, inline), user);
                        /* if (id != 0)
                        Delete(id, user);*/

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
                            {"remove_keyboard", true}
                        }.ToString();
                        
                        chat.KeyboardReset();
                        continue;
                    }
                    
                    if (attachment is InlineMarkupKeyboard inlineMarkupKeyboard)
                    {
                        keyboard = inlineMarkupKeyboard.ToString(user, response.Text, Provider);
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

            if (keyboard == null && chat.ReplyMarkupKeyboard?.Pages?.Count != null)
            {
                keyboard = Provider.Keyboard.BuildReplyMarkupKeyboard(
                    chat.ReplyMarkupKeyboard.Menu, 
                    chat.ReplyMarkupKeyboard.Pages[chat.KeyboardPage]).ToString();
            }

            if (peerId == 0) peerId = (long)user.Id;
            var args = new List<WebMultipartContent>()
            {
                new WebMultipartContent("chat_id", new StringContent(peerId.ToString())),
            };

            if (parseMode != null)
                args.Add(new WebMultipartContent("parse_mode", new StringContent(parseMode)));

            if (keyboard != null)
                args.Add(new WebMultipartContent("reply_markup", new StringContent(keyboard)));

            JToken request;
            
            if (attachments.Count == 0)
            {
                if (string.IsNullOrEmpty(response.Text))
                {
                    response.Text = "\u2062";
                } 
                
                args.Add(new WebMultipartContent("text", new StringContent(response.Text)));
                
                request = (Provider as TelegramApiProvider).SendMultipartRequest("sendMessage", args, false);
                if (request == null) return 0;

                return (int) request["message_id"];
            }

            var media = GetMediaGroup(typeAttachment);
            if (attachments.Count >= 2)
            {
                var jArray = new JArray();
                var i = 0;
                
                foreach (var attachment in attachments)
                {
                    var obj = new JObject
                    {
                        {"type", media.Type.ToLower()},
                    };
                    if (i == 0) obj.Add(media.ParameterText, response.Text);

                    if (attachment.Item2 is byte[] bytes)
                    {
                        args.Add(new WebMultipartContent($"photo{i}", new StreamContent(new MemoryStream(bytes))));
                        obj.Add("media", $"attach://photo{i}");
                    }
                    else
                        obj.Add("media", attachment.Item2.ToString());

                    jArray.Add(obj);
                    i++;
                }
                
                args.Add(new WebMultipartContent("media", new StringContent(jArray.ToString())));

                request = (Provider as TelegramApiProvider).SendMultipartRequest("sendMediaGroup", args, false);
                if (request == null) return 0;
                else if (!((request as JObject)?.ContainsKey("message_id") ?? false)) return 0;

                return (int) request["message_id"];
            }
            
            if (!string.IsNullOrEmpty(response.Text))
                args.Add(new WebMultipartContent(media.ParameterText, new StringContent(response.Text)));
            
            if (attachments[0].Item2 is byte[] bytesAttachment)
                args.Add(new WebMultipartContent(
                    attachments[0].Item1, new StreamContent(new MemoryStream(bytesAttachment))));
            else
                args.Add(new WebMultipartContent(
                    attachments[0].Item1, new StringContent(attachments[0].Item2.ToString())));

            request = (Provider as TelegramApiProvider).SendMultipartRequest($"send{media.Type}", args, false);
            if (request == null) return 0;

            return (int)request["message_id"];
        }
        
        public bool AnswerCallbackQuery(string id, string message = null)
        {
            var args = new Dictionary<string, string>
            {
                {"callback_query_id", id}
            };
                
            if (message != null) args.Add("text", message);
                
            return Provider.SendRequest("answerCallbackQuery", args, false) != null;
        }

        public bool Delete(int messageId, User user)
        {
            return Provider.SendRequest("deleteMessage", new Dictionary<string, string>
            {
                {"chat_id", user.Id.ToString()},
                {"message_id", messageId.ToString()}
            }, false) != null;
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