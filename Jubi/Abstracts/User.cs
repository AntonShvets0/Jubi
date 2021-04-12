using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Jubi.Api;
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

        public void KeyboardReset()
        {
            KeyboardPage = 0;
            Keyboard = null;
        }
        

        public bool Send(Message message)
            => Provider.Api.Messages.Send(message, this);

        public SiteProvider Provider;

        internal bool IsWaitingResponse;
        internal string ResponseLine;

        public string ReadString()
        {
            IsWaitingResponse = true;
            ResponseLine = null;
            
            while (ResponseLine == null) {}

            IsWaitingResponse = false;

            var tmp = ResponseLine;
            ResponseLine = null;
            
            return tmp;
        }

        public string ReadString(Message message)
        {
            Send(message);
            return ReadString();
        }

        public delegate bool ConvertDelegate<T>(string str, out T result);
        
        public T ReadAndConvertToType<T>(ConvertDelegate<T> tryParseFunction, string error)
        {
            while (true)
            {
                var data = ReadString();
                if (tryParseFunction(data, out T result)) return result;
                Send(error);
            }
        }

        public T ReadAndConvertToType<T>(Message message, ConvertDelegate<T> tryParseFunction, string error)
        {
            Send(message);
            return ReadAndConvertToType(tryParseFunction, error);
        }
        
        public int ReadInt()
            => ReadAndConvertToType<int>(
                int.TryParse, 
                Provider.BotInstance.Configuration["error"]["int_convert"].ToString());

        public int ReadInt(Message message)
            => ReadAndConvertToType<int>(
                message,
                int.TryParse, 
                Provider.BotInstance.Configuration["error"]["int_convert"].ToString());
        
        public double ReadDouble()
            => ReadAndConvertToType<double>(
                double.TryParse, 
                Provider.BotInstance.Configuration["error"]["int_convert"].ToString());

        public double ReadDouble(Message message)
            => ReadAndConvertToType<double>(
                message,
                double.TryParse, 
                Provider.BotInstance.Configuration["error"]["int_convert"].ToString());
        
        public bool ReadBoolean()
            => ReadAndConvertToType<bool>(
                bool.TryParse, 
                Provider.BotInstance.Configuration["error"]["bool_convert"].ToString());

        public bool ReadBoolean(Message message)
            => ReadAndConvertToType<bool>(
                message,
                bool.TryParse, 
                Provider.BotInstance.Configuration["error"]["bool_convert"].ToString());
    }
}