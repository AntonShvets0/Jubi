namespace Jubi.Updates.Types
{
    public class MessageNewContent : IUpdateContent
    {
        public long PeerId { get; set; }
        
        public string Text { get; set; }

        public string Payload { get; set; }
        
        public string QueryData { get; set; } 
    }
}