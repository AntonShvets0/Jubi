using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.Response;

namespace Jubi.Console.Api.Types
{
    public class ConsoleMessageApiProvider : IMessageApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit()
        {
            
        }

        public bool Send(Message response, User user)
        {
            return true;
        }
    }
}