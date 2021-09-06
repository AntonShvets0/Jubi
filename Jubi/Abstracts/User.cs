using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jubi.Api;
using Jubi.Chats;
using Jubi.Models;
using Jubi.Response;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;
using Newtonsoft.Json.Linq;

namespace Jubi.Abstracts
{
    public abstract class User
    {
        public ulong Id;
        public int Page = 0;
        
        public List<Action> ThreadPoolActions = new List<Action>();
        public bool IsExecuting = false;

        private object _userLock = new object();

        public object ThreadPoolLock = new object();
        
        protected static Dictionary<Type, ReadMessageData> _readMessageData;

        public SiteProvider Provider;

        public List<UserChat> Chats { get; } = new List<UserChat>();

        public long LastPeerId { get; set; }
        
        public string QueryId { get; set; }

        public UserChat GetChat(long peerId = 0)
        {
            lock (_userLock)
            {
                var chat = Chats.FirstOrDefault(c => c.PeerId == (peerId == 0 ? (long)Id : peerId));

                if (chat == null)
                {
                    chat = new UserChat(peerId == 0 ? (long)Id : peerId);
                    Chats.Add(chat);
                }

                return chat;
            }
        }
        
        public virtual int Send(Message message, long peerId = 0)
        {
            message.Text = ProccessText(message.Text);
            return Provider.Api.Messages.Send(message, this, peerId);
        }

        private string ProccessText(string text)
        {
            if (text == null) return null;
            
            text = text.Replace("<<", "«");
            text = text.Replace(">>", "»");
            text = text.Replace("--", "—");
            
            return text;
        }
        
        public void Read<T>(Action<T> newMessage, ReadMessageData messageData = null, long peerId = 0)
        {
            if (_readMessageData == null)
                FillReadMessageData(Provider.BotInstance);
            if (peerId == 0) peerId = (long)Id;

            GetChat(peerId).NewMessageAction = message =>
            {
                ReadMessageData data;

                if (messageData != null)
                    data = messageData;
                else if (_readMessageData.ContainsKey(typeof(T)))
                    data = _readMessageData[typeof(T)];
                else
                    throw new InvalidCastException("Unknown type");

                if (!data.TryParse(message, out object result))
                {
                    Send(data.Error, peerId);
                    Read(newMessage, messageData);
                    return;
                }

                GetChat(peerId).NewMessageAction = null;
                newMessage?.Invoke((T) result);
            };
        }

        public void Read<T>(Message message, Action<T> newMessage, ReadMessageData messageData = null, long peerId = 0)
        {
            if (peerId == 0) peerId = (long)Id;
            Send(message, peerId);
            Read<T>(newMessage, messageData, peerId);
        }

        public void Read(Message message, Action<string> newMessage, long peerId = 0)
        {
            Read<string>(message, newMessage, null, peerId);
        }

        public void Read(Action<string> newMessage, long peerId = 0)
        {
            Read<string>(newMessage, null, peerId);
        }

        protected virtual void FillReadMessageData(Bot bot)
        {
            _readMessageData = new Dictionary<Type, ReadMessageData>
            {
                {typeof(int), new ReadMessageData<int>(Error.FromConfig(bot, "int_convert"), int.TryParse)},
                {typeof(long), new ReadMessageData<long>(Error.FromConfig(bot, "int_convert"), long.TryParse)},
                {typeof(decimal), new ReadMessageData<decimal>(Error.FromConfig(bot, "int_convert"), decimal.TryParse)},
                {typeof(bool), new ReadMessageData<bool>(Error.FromConfig(bot, "bool_convert"), bool.TryParse)},

                {typeof(double), new ReadMessageData<double>(Error.FromConfig(bot, "int_convert"), double.TryParse)},
                {typeof(float), new ReadMessageData<float>(Error.FromConfig(bot, "int_convert"), float.TryParse)},

                {typeof(uint), new ReadMessageData<uint>(Error.FromConfig(bot, "uint_convert"), uint.TryParse)},
                {typeof(ulong), new ReadMessageData<ulong>(Error.FromConfig(bot, "uint_convert"), ulong.TryParse)},

                {typeof(string), new ReadMessageData<string>(null,
                    (string str, out string result) =>
                    {
                        result = str;
                        return true;
                    })}
            };
        }
    }
}