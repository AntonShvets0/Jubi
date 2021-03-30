using Jubi.Abstracts;
using Jubi.Response;
using Jubi.Response.Attachments;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;

namespace Jubi.ConsoleApp.Executors
{
    public class HelloExecutor : CommandExecutor
    {
        public override string Alias { get; } = "hello";

        public override Message? Execute(User user, string[] args)
        {
            var keyboard = new ReplyMarkupKeyboard();
            keyboard.AddButton("Приветствую!", "hi", KeyboardColor.Green);
            keyboard.AddPage();
            keyboard.AddButton("Приветствую!", "hi", KeyboardColor.Primary);
            keyboard.AddPage();
            keyboard.AddButton("Приветствую!", "hi", KeyboardColor.Red);

            return new Message(
                "Салам, славяне!", 
                new IAttachment[]
                {
                    keyboard, 
                    new PhotoAttachment("https://sun9-3.userapi.com/impg/lCTtJJlI-fPpcAGnXyynuR3JcxQRIIEFz-JNdg/luK9Hvhve2c.jpg?size=743x196&quality=96&sign=c29891f46f0896b082e7026e06821ce4&type=album")
                });
        }
    }
}