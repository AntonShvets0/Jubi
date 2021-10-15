using System;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Response.Attachments.Keyboard.Models;
using Jubi.Response.Attachments.Keyboard.Parameters;
using Jubi.Response.Interfaces;

namespace Jubi.Response.Attachments.Keyboard
{
    public class InlineMarkupKeyboard : AbstractKeyboard, IAttachment
    {
        public InlineMarkupKeyboard()
        {
            
        }

        public InlineMarkupKeyboard(InlineMarkupKeyboard copy, int pageIndex = 0)
        {
            IsOneTime = copy.IsOneTime;
            Pages = copy.Pages;

            PageIndex = pageIndex;
        }
        
        public void AddButton(string name, Action<User> action, User user, KeyboardColor color = KeyboardColor.Default)
        {
            AddButton(name, new DefaultButtonAction
            {
                Action = action,
                Executor = $"from {user.Id}"
            }, color);
        }
        
        public override string ToString(User user, string message, IApiProvider site)
        {
            if (base.ToString(user, message, site) == null) return null;

            user.GetChat(user.LastPeerId).InlineMarkupKeyboard = this;

            return site.Keyboard.BuildInlineMarkupKeyboard(Pages[PageIndex]).ToString();
        }
    }
}