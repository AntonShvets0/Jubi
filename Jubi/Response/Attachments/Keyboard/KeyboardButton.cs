using System;
using Jubi.Response.Attachments.Keyboard.Parameters;


namespace Jubi.Response.Attachments.Keyboard
{
    public class KeyboardButton
    {
        public string Name { get; set; }
        
        public IKeyboardAction Action { get; set; }
        
        public KeyboardColor Color { get; set; }
    }
}