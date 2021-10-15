using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Response.Attachments.Keyboard.Models;
using Jubi.Response.Interfaces;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.Response.Attachments.Keyboard
{
    public class ReplyMarkupKeyboard : AbstractKeyboard, IAttachment
    {
        public readonly KeyboardButton Menu;
        
        public virtual bool IsEmpty { get; } = false;

        public ReplyMarkupKeyboard(bool isOneTime = false, KeyboardButton menu = null)
        {
            IsOneTime = isOneTime;
            Menu = menu;
        }

        public ReplyMarkupKeyboard(bool isOneTime, Action action)
        {
            IsOneTime = isOneTime;
            Menu = new KeyboardButton
            {
                Name = MenuText,
                Action = new DefaultButtonAction
                {
                    Action = _ => action.Invoke()
                }
            };
        }
        
        public ReplyMarkupKeyboard(bool isOneTime, string executor)
        {
            IsOneTime = isOneTime;
            Menu = new KeyboardButton
            {
                Name = MenuText,
                Action = new DefaultButtonAction
                {
                    Executor = executor
                }
            };
        }
        
        public ReplyMarkupKeyboard(ReplyMarkupKeyboard copy, int pageIndex = 0)
        {
            IsOneTime = copy.IsOneTime;
            Pages = copy.Pages;
            Menu = copy.Menu;

            PageIndex = pageIndex;
        }

        public override string ToString(User user, string message, IApiProvider site)
        {
            if (base.ToString(user, message, site) == null) return null;
            if (IsEmpty) return null;
            
            user.GetChat(user.LastPeerId).ReplyMarkupKeyboard = this;

            return site.Keyboard.BuildReplyMarkupKeyboard(Menu, Pages[PageIndex], IsOneTime).ToString();
        }
    }
}