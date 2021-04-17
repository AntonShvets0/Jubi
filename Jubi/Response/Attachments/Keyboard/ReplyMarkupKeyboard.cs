using System;
using System.Collections.Generic;
using System.Linq;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Response.Interfaces;

namespace Jubi.Response.Attachments.Keyboard
{
    public class ReplyMarkupKeyboard : IAttachment
    {
        public int MaxInRows = 4;
        public int MaxRows = 6;

        public readonly KeyboardAction Menu;
        public readonly bool IsOneTime;

        public static string PreviousButtonText;
        public static string NextButtonText;
        public static string MenuText;
        
        public virtual bool IsEmpty { get; } = false;

        public List<KeyboardPage> Pages = new List<KeyboardPage>
        {
            new KeyboardPage
            {
                Rows = new List<KeyboardRow>
                {
                    new KeyboardRow()
                }
            }
        };

        private readonly int PageIndex;
        
        public ReplyMarkupKeyboard(bool isOneTime = false, KeyboardAction keyboard = null)
        {
            IsOneTime = isOneTime;
            Menu = keyboard;
        }

        public ReplyMarkupKeyboard(bool isOneTime, Action action)
        {
            IsOneTime = isOneTime;
            Menu = new KeyboardAction
            {
                Name = MenuText,
                Action = action
            };
        }
        
        public ReplyMarkupKeyboard(bool isOneTime, string executor)
        {
            IsOneTime = isOneTime;
            Menu = new KeyboardAction
            {
                Name = MenuText,
                Executor = executor
            };
        }
        
        public ReplyMarkupKeyboard(ReplyMarkupKeyboard copy, int pageIndex = 0)
        {
            IsOneTime = copy.IsOneTime;
            Pages = copy.Pages;
            Menu = copy.Menu;

            PageIndex = pageIndex;
        }

        public void AddButton(string name, string executor, KeyboardColor color = KeyboardColor.Default)
        {
            if (MaxInRows < Pages.Last().Rows.Last().Buttons.Count + 1) AddLine();
            Pages.Last().Rows.Last().Buttons.Add(new KeyboardAction
            {
                Name = name,
                Executor = executor,
                Color = color
            });
        }

        public void AddButton(string name, Action action, KeyboardColor color = KeyboardColor.Default)
        {
            if (MaxInRows < Pages.Last().Rows.Last().Buttons.Count + 1) AddLine();
            Pages.Last().Rows.Last().Buttons.Add(new KeyboardAction
            {
                Name = name,
                Action = action,
                Color = color
            });
        }

        public void AddPage()
        {
            if (Pages.Count > 1)
            {
                Pages.Last().Rows.Add(new KeyboardRow
                {
                    Buttons = new List<KeyboardAction>
                    {
                        new KeyboardAction
                        {
                            Name = PreviousButtonText, 
                            Color = KeyboardColor.Primary, 
                            Executor = "/page previous"
                        }
                    }
                });
            }
            
            if (Pages.Count >= 2)
            {
                if (Pages[Pages.Count - 2].Rows.Last().Buttons.Last().Executor != "/page previous")
                {
                    Pages[Pages.Count - 2].Rows.Add(new KeyboardRow());
                }
                
                Pages[Pages.Count - 2].Rows.Last().Buttons.Add(new KeyboardAction
                {
                    Name = NextButtonText, Color = KeyboardColor.Primary, Executor = "/page next"
                });
            }

            Pages.Add(new KeyboardPage
            {
                Rows = new List<KeyboardRow>
                {
                    new KeyboardRow()
                }
            });
        }

        public void AddLine()
        {
            if (MaxRows < Pages.Last().Rows.Count + 1)
            {
                AddPage();
                return;
            }
            
            Pages.Last().Rows.Add(new KeyboardRow());
        }

        public string ToString(User user, IApiProvider site)
        {
            if (IsEmpty) return null;

            if (Pages.LastOrDefault()?.Rows?.LastOrDefault()?.Buttons?.LastOrDefault()?.Executor != "/page previous")
            {
                AddPage();
                Pages.RemoveAt(Pages.Count - 1);
            }

            if (PageIndex > Pages.Count - 1 || PageIndex < 0) return null;
            
            user.KeyboardPage = PageIndex;
            user.Keyboard = this;
            
            return site.BuildKeyboard(Menu, Pages[PageIndex], IsOneTime).ToString();
        }
    }
}