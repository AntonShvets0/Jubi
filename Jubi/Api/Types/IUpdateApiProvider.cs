using System.Collections;
using System.Collections.Generic;
using Jubi.Updates;

namespace Jubi.Api.Types
{
    public interface IUpdateApiProvider : IMethodApiProvider
    {
        IEnumerable<UpdateInfo> Get();
    }
}