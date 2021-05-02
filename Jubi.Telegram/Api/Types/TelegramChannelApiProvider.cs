using System.Collections.Generic;
using Jubi.Api;
using Jubi.Api.Types;

namespace Jubi.Telegram.Api.Types
{
    public class TelegramChannelApiProvider : IMethodApiProvider
    {
        public IApiProvider Provider { get; set; }

        public void OnInit() { }

        public bool IsMember(string chatId, ulong member)
        {
            try
            {
                Provider.SendRequest("getChatMember", new Dictionary<string, string>
                {
                    {"chat_id", chatId},
                    {"user_id", member.ToString()}
                }, true);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}