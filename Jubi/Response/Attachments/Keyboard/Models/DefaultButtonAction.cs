using System;
using Jubi.Abstracts;

namespace Jubi.Response.Attachments.Keyboard.Models
{
    public class DefaultButtonAction : IKeyboardAction
    {
        public Action<User> Action { get; set; }
        public string Executor { get; set; }
    }
}