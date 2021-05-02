using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jubi.Api;
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

        public object ThreadPoolLock = new object();

        public ReplyMarkupKeyboard Keyboard;
        public int KeyboardPage = 0;
        
        protected static Dictionary<Type, ReadMessageData> _readMessageData;

        public SiteProvider Provider;

        public void KeyboardReset()
        {
            KeyboardPage = 0;
            Keyboard = null;
        }

        public virtual bool Send(Message message)
            => Provider.Api.Messages.Send(message, this);

        public Action<string> NewMessageAction;
        
        public void Read<T>(Action<T> newMessage, ReadMessageData messageData = null)
        {
            if (_readMessageData == null)
                FillReadMessageData(Provider.BotInstance);

            NewMessageAction = message =>
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
                    Send(data.Error);
                    Read(newMessage, messageData);
                    return;
                }

                NewMessageAction = null;
                newMessage?.Invoke((T) result);
            };
        }

        public void Read<T>(Message message, Action<T> newMessage, ReadMessageData messageData = null)
        {
            Send(message);
            Read<T>(newMessage, messageData);
        }

        public void Read(Message message, Action<string> newMessage)
        {
            Read<string>(message, newMessage);
        }

        public void Read(Action<string> newMessage)
        {
            Read<string>(newMessage);
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