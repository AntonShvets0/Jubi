using System;

namespace Jubi.Attributes
{
    public class CommandAttribute : Attribute
    {
        public string Alias { get; }

        public CommandAttribute(string alias)
        {
            Alias = alias;
        }
    }
}