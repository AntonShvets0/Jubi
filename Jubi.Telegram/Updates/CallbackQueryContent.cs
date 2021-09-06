using Jubi.Updates;

namespace Jubi.Telegram.Updates
{
    public class CallbackQueryContent : IUpdateContent
    {
        public string Id { get; set; }
        
        public string Data { get; set; }
        
        public long PeerId { get; set; }
    }
}