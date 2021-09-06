namespace Jubi.Response.Attachments.Keyboard.Models
{
    public class LinkButtonAction : IKeyboardAction
    {
        public string Url { get; set; }

        public LinkButtonAction(string url)
        {
            Url = url;
        }
    }
}