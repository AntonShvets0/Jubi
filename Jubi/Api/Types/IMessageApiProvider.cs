using Jubi.Abstracts;
using Jubi.Response;

namespace Jubi.Api.Types
{
    public interface IMessageApiProvider : IMethodApiProvider
    { 
        int Send(Message response, User user, long peerId = 0);

        bool Delete(int messageId, User user);
    }
}