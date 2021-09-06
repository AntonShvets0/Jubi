using System;
using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Widget
{
    public class WidgetFooter
    {
        public string Title { get; }
        
        public string Url { get; }

        public WidgetFooter(string title, string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException();
            
            Title = title;
            Url = url;
        }
        
        public JObject ToJson()
        {
            var jObject = new JObject
            {
                {"more", Title},
                {"more_url", Url}
            };
            
            return jObject;
        }
    }
}