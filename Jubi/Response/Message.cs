using System;
using Jubi.Response.Attachments;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;

namespace Jubi.Response
{
    public struct Message
    {
        public string Text;
        public IAttachment[] Attachments;

        public Message(string text, params IAttachment[] attachments)
        {
            Text = text;
            Attachments = attachments;
        }

        private static Message FromIAttachment(IAttachment attachment)
        {
            return new Message { Attachments = new []{ attachment }};
        }
        
        public static implicit operator Message(ReplyMarkupKeyboard attachment) => FromIAttachment(attachment);
        public static implicit operator Message(PhotoAttachment attachment) => FromIAttachment(attachment);

        public static implicit operator Message(string message) 
            => new Message { Text = message };
        
        public static implicit operator Message(IAttachment[] attachments)
            => new Message {Attachments = attachments};
    }
}