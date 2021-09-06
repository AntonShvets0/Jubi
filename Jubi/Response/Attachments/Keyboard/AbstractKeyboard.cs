using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Response.Attachments.Keyboard.Models;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.Response.Attachments.Keyboard
{
    public abstract class AbstractKeyboard
    {
        public static string PreviousButtonText;
        public static string NextButtonText;
        public static string MenuText;
        
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
        
        public int MaxInRows = 4;
        public int MaxRows = 6;

        public bool IsOneTime { get; protected set; }

        protected int PageIndex { get; set; }

        public virtual string ToString(User user, string message, IApiProvider site)
        {
            var button = Pages.LastOrDefault()?.Rows?.LastOrDefault()?.Buttons?.LastOrDefault();
            if (button?.Action is DefaultButtonAction defaultButton)
            {
                if (defaultButton.Executor != "/page previous")
                {
                    AddPage();
                    Pages.RemoveAt(Pages.Count - 1);
                }
            }
            
            if (PageIndex > Pages.Count - 1 || PageIndex < 0) return null;
            
            user.GetChat().KeyboardPage = PageIndex;
            user.GetChat().KeyboardMessage = message;

            return "";
        }
        
        public void AddButton(string name, IKeyboardAction action, KeyboardColor color = KeyboardColor.Default)
        {
            if (MaxInRows < Pages.Last().Rows.Last().Buttons.Count + 1) AddLine();
            name = ProcessName(name);
            
            Pages.Last().Rows.Last().Buttons.Add(new KeyboardButton
            {
                Name = name,
                Action = action,
                Color = color
            });
        }

        public void AddButton(string name, string executor, KeyboardColor color = KeyboardColor.Default)
        {
            AddButton(name, new DefaultButtonAction
            {
                Executor = executor
            }, color);
        }

        private string ProcessName(string name)
        {
            var match = Regex.Match(name, @"^(.*) \(x(.*)\)$");
            if (match.Groups.Count == 3)
            {
                var itemName = match.Groups[1].Value;
                var count = match.Groups[2].Value;

                if (itemName.Length > 33)
                {
                    itemName = itemName.Substring(0, 30) + "...";
                }

                name = $"{itemName} (x{count})";
            }
            else if (name.Length > 40)
            {
                name = name.Substring(0, 37) + "...";
            }

            return name;
        }

        public void AddButton(string name, Action action, KeyboardColor color = KeyboardColor.Default)
        {
            AddButton(name, new DefaultButtonAction
            {
                Action = _ => action.Invoke()
            }, color);
        }

        public void AddPage()
        {
            if (Pages.Count > 1)
            {
                Pages.Last().Rows.Add(new KeyboardRow
                {
                    Buttons = new List<KeyboardButton>
                    {
                        new KeyboardButton
                        {
                            Name = PreviousButtonText, 
                            Color = KeyboardColor.Primary, 
                            Action = new DefaultButtonAction
                            {
                                Executor = "/page previous"
                            }
                        }
                    }
                });
            }
            
            if (Pages.Count >= 2)
            {
                var button = Pages[Pages.Count - 2].Rows.Last().Buttons.Last();
                if (button.Action is DefaultButtonAction defaultButtonAction)
                {
                    if (defaultButtonAction.Executor != "/page previous") 
                        Pages[Pages.Count - 2].Rows.Add(new KeyboardRow());
                }
                
                Pages[Pages.Count - 2].Rows.Last().Buttons.Add(new KeyboardButton
                {
                    Name = NextButtonText, 
                    Color = KeyboardColor.Primary, 
                    Action = new DefaultButtonAction
                    {
                        Executor = "/page next"
                    }
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
    }
}