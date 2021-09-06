namespace Jubi.VKontakte.Widget.Table
{
    public class Column
    {
        public string Name { get; }
        
        public string Url { get; }
        
        public string Icon { get; }

        public Column(string name, string icon = null, string url = null)
        {
            Name = name;
            Url = url;
            Icon = icon;
        }

        public static implicit operator Column(string el) => new Column(el);
    }
}