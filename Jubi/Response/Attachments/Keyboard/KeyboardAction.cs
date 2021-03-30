using System;

namespace Jubi.Response.Attachments.Keyboard
{
    public class KeyboardAction
    {
        public string Name;
        
        public Action Action;
        public string Executor;

        public KeyboardColor Color;
    }
}