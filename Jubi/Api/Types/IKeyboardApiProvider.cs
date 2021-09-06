using Jubi.Response.Attachments.Keyboard;
using Newtonsoft.Json.Linq;
using Jubi.Response.Attachments.Keyboard.Parameters;

namespace Jubi.Api.Types
{
    public interface IKeyboardApiProvider : IMethodApiProvider
    {
        JObject BuildReplyMarkupKeyboard(KeyboardButton menu, KeyboardPage keyboard, bool isOneTime = false);

        JObject BuildInlineMarkupKeyboard(KeyboardPage keyboard);
    }
}