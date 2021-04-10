using System;
using System.Collections.Generic;
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

        public string ReadLine()
        {
            IsWaitingResponse = true;
            ResponseLine = null;
            
            while (ResponseLine == null) {}

            IsWaitingResponse = false;

            var tmp = ResponseLine;
            ResponseLine = null;
            
            return tmp;
        }

        public string ReadLine(Message message)
        {
            Send(message);
            return ReadLine();
        }
    }
}