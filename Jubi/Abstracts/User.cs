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
        
        public object ThreadLockUser = new object();

        public ReplyMarkupKeyboard Keyboard;
        public int KeyboardPage = 0;
        
        private static Dictionary<Type, ReadMessageData> _readMessageData;

        public SiteProvider Provider;

        internal bool IsWaitingResponse;
        internal string ResponseLine;

        public void KeyboardReset()
        {
            KeyboardPage = 0;
            Keyboard = null;
        }

        public bool Send(Message message)
            => Provider.Api.Messages.Send(message, this);
        
        public T Read<T>(ReadMessageData messageData = null)
        {
            if (_readMessageData == null) 
                FillReadMessageData(Provider.BotInstance);

            while (true)
            {
                IsWaitingResponse = true;
                ResponseLine = null;
            
                while (ResponseLine == null) {}

                IsWaitingResponse = false;

                var tmp = ResponseLine;
                ResponseLine = null;
                
                ReadMessageData data;

                if (messageData != null)
                    data = messageData;
                else if (_readMessageData.ContainsKey(typeof(T)))
                    data = _readMessageData[typeof(T)];
                else
                    throw new InvalidCastException("Unknown type");

                if (!data.TryParse(tmp, out object result))
                    Send(messageData.Error);

                return (T) result;
            }
        }

        public T Read<T>(Message message, ReadMessageData messageData = null)
        {
            Send(message);
            return Read<T>(messageData);
        }

        public string Read(Message? message = null)
        {
            if (message.HasValue) return Read<string>(message.Value);

            return Read<string>();
        }

        private static void FillReadMessageData(Bot bot)
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