# Jubi
Jubi - library for creating multiplatform bots in C#. Two platforms are now available: VKontakte and Telegram. However, you can add your own platform. Through Jubi, this is done in a few hundred lines.


## Requirements
* .NET Core 5.0
* SimpleIni 1.1
* NewtonSoft.Json 13.0.1

## Hello World in Jubi

Program.cs:
```C#

var bot = new Bot("path to config.ini", new SiteProvider[] {
new VKontakteProvider(), new TelegramProvider() // what platforms should be connected
}, new ExecutorInformation {
    Namespace = "Example.Executors", // where commands for the bot are stored (Jubi creates class instances through reflection)
    Assembly = Assembly.GetExecutingAssembly()
});

bot.Start();

```

Executors/StartExecutor.cs:
```C#
namespace Example.Executors 
{
    public class StartExecutor : CommandExecutor
    {
        public override string Alias { get; } = "start";
        
        public override Message? Execute(User user, string[] args)
        {
            return "Hello world!";
        }
    }
}

```

This code start bot in Telegram and VKontakte with one command /start, who send "Hello world!" to user.


### Keyboards
```C#

var keyboard = new ReplyMarkupKeyboard();
keyboard.AddButton("Hello World", () => {}, KeyboardColor.Primary); // if the platform supports colors for buttons, then they will be the color, which passed as 3 argument.
keyboard.AddButton("Run /menu command", "/menu", KeyboardColor.Red);

keyboard.AddPage(); // Keyboards in Jubi supported pagination. You can create as many buttons as you like!
keyboard.AddLine(); // Add new line

return new Message("Text for user", new [] { keyboard }); // send message with keyboard
return new Message("This message delete keyboard", new [] { new ReplyMarkupKeyboard() });

```

### Attachments
Jubi currently only supports images as attachments. In the future, I will created classes for other attachments.
```C#
return new Message("Photo", new [] { new PhotoAttachment("link to photo, or path") });

return new Message("Photo", new [] { new PhotoAttachment(bytes) });

```