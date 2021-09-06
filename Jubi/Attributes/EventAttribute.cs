using System;

namespace Jubi.Attributes
{
    public class EventAttribute : Attribute
    {
        public string Event { get; }
        
        public EventAttribute(string ev)
        {
            Event = ev;
        }
    }
}