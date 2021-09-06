using System;

namespace Jubi.VKontakte.Widget
{
    public abstract class AppWidget
    {
        public abstract string Type { get; }
        
        public override string ToString() => throw new NotImplementedException();
    }
}