using Newtonsoft.Json.Linq;

namespace Jubi.VKontakte.Widget
{
    public class WidgetTitle
    {
        public string Title { get; }
        
        public string Url { get; }
        
        public int Counter { get; }

        public WidgetTitle(string title, string titleUrl = null)
        {
            Title = title;
            Url = titleUrl;
        }

        public WidgetTitle(string title, int titleCounter, string titleUrl)
        {
            Title = title;
            Counter = titleCounter;
            Url = titleUrl;
        }

        public JObject ToJson()
        {
            var jObject = new JObject
            {
                {"title", Title}
            };

            if (Url != null)
                jObject.Add("title_url", Url);
            
            if (Counter != null)
                jObject.Add("title_counter", Counter);
            
            return jObject;
        }
    }
}