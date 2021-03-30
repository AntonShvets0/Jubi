﻿using System;
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

        public static string PreviousButtonText;
        public static string NextButtonText;

        public bool IsEmpty => Pages.Count == 0;

        private List<KeyboardPage> Pages = new List<KeyboardPage>
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

        public ReplyMarkupKeyboard() {}
        
        public ReplyMarkupKeyboard(List<KeyboardPage> pages, int index)
        {
            Pages = pages;
            PageIndex = index;
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
                            Executor = "page previous"
                        }
                    }
                });
            }
            
            if (Pages.Count >= 2)
            {
                if (Pages[Pages.Count - 2].Rows.Last().Buttons.Last().Executor != "page previous")
                {
                    Pages[Pages.Count - 2].Rows.Add(new KeyboardRow());
                }
                
                Pages[Pages.Count - 2].Rows.Last().Buttons.Add(new KeyboardAction
                {
                    Name = NextButtonText, Color = KeyboardColor.Primary, Executor = "page next"
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
            if (Pages.Count > 0 && Pages.Last().Rows.Last().Buttons.Last().Executor != "page previous")
            {
                AddPage();
                Pages.RemoveAt(Pages.Count - 1);
            }
            
            if (PageIndex > Pages.Count - 1 || PageIndex < 0) return null;
            
            user.KeyboardPage = PageIndex;
            user.Keyboard = Pages;
            
            return site.BuildKeyboard(Pages[PageIndex]).ToString();
        }
    }
}