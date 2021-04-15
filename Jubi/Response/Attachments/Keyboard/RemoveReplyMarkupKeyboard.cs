namespace Jubi.Response.Attachments.Keyboard
{
    public class RemoveReplyMarkupKeyboard : ReplyMarkupKeyboard
    {
        public override bool IsEmpty { get; protected set; } = true;
    }
}