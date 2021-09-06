using System;
using Jubi.Response.Attachments.Keyboard;

namespace Jubi.Chats
{
    public class UserChat
    {
        public long PeerId { get; set; }
        
        private ReplyMarkupKeyboard _replyMarkup;

        public ReplyMarkupKeyboard ReplyMarkupKeyboard
        {
            get => _replyMarkup;
            set
            {
                _replyMarkup = value;
                InlineMarkupKeyboard = null;
            }
        }
        public InlineMarkupKeyboard InlineMarkupKeyboard { get; set; }
        
        public string KeyboardMessage { get; set; }
        public int KeyboardPage { get; set; } = 0;
        
        public bool HasKeyboard() => ReplyMarkupKeyboard != null || InlineMarkupKeyboard != null;

        public Action<string> NewMessageAction;

        public void KeyboardReset()
        {
            KeyboardPage = 0;
            KeyboardMessage = null;
            ReplyMarkupKeyboard = null;
            InlineMarkupKeyboard = null;
        }

        public UserChat(long peerId)
        {
            PeerId = peerId;
        }
    }
}