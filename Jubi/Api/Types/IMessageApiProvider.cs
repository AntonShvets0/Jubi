using Jubi.Abstracts;
using Jubi.Response;

namespace Jubi.Api.Types
{
    public interface IMessageApiProvider : IMethodApiProvider
    { 
        bool Send(Message response, User user);
    }
}