using System.Collections.Generic;
using Jubi.Api;
using Jubi.Api.Types;
using Jubi.VKontakte.Widget;

namespace Jubi.VKontakte.Api.Types
{
    public class VKontakteWidgetApiProvider : IMethodApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit() { }

        public void Update(AppWidget widget, string token)
        {
            Provider.SendRequest("appWidgets.update", new Dictionary<string, string>
            {
                {"type", widget.Type},
                {"code", widget.ToString()},
                {"access_token", token}
            });
        }
    }
}