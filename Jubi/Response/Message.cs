﻿using System;
using Jubi.Response.Interfaces;

namespace Jubi.Response
{
    public struct Message
    {
        public string Text;
        public IAttachment[] Attachments;

        public Message(string text, IAttachment[] attachments)
        {
            
            Text = text;
            Attachments = attachments;
        }

        public static implicit operator Message(string message) 
            => new Message { Text = message };
    }
}