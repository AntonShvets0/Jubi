using System.Net.Http;

namespace Jubi.Api
{
    public class WebMultipartContent
    {
        public HttpContent Content;
        public string Name;

        public WebMultipartContent(string name, HttpContent content)
        {
            Name = name;
            Content = content;
        }
    }
}