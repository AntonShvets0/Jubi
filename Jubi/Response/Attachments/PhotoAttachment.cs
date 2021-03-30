using Jubi.Response.Interfaces;

namespace Jubi.Response.Attachments
{
    public class PhotoAttachment : IAttachment
    {
        public byte[] Content;
        public string Url;

        public PhotoAttachment(string url)
        {
            Url = url;
        }

        public PhotoAttachment(byte[] content)
        {
            Content = content;
        }
    }
}