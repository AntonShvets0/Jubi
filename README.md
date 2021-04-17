# Jubi
Jubi - library for creating multiplatform bots in C#. Two platforms are now available: VKontakte and Telegram. However, you can add your own platform. Through Jubi, this is done in a few hundred lines.


## Requirements
* .NETStandard 2.1
* SimpleIni 3.0.1
* NewtonSoft.Json 13.0.1

## Hello World in Jubi

Program.cs:
```C#

var bot = BotBuilder.Create("path to config.ini", new SiteProvider[] {
new VKontakteProvider(), new TelegramProvider() // what platforms should be connected
}, new ExecutorInformation {
    Namespace = "Example.Executors", // where commands for the bot are stored (Jubi creates class instances through reflection)
    Assembly = Assembly.GetExecutingAssembly()
});

bot.Run();

```

Executors/StartExecutor.cs:
```C#
namespace Example.Executors 
{
    [Command("start")]
    public class StartExecutor : CommandExecutor
    {        
        public override Message? Execute()
        {
            return "Hello world!";
        }
    }
}

```

This code start bot in Telegram and VKontakte with one command /start, who send "Hello world!" to user.
If Execute() return null - message will not be sent.
Also, you can send a message like this:
```C#
User.Send(Message);
```

CommandExecutor contains properties:
```C#

// User, who call this command
public User User { get; set; }

// Args
public object[] Args { get; set; }

// If true, this command can be run only from keyboard
public bool IsHidden { get; }

// Parent. If it's subcommand - this properties will not be null
public CommandExecutor Parent { get; set; }

// Instance of main class library
public Bot BotInstance { get; set; }

```

### Typechecking args and automatic cast

The argument is received through the method Get(index). It's return string... but, what if you need to get a number? It was possible to write own checks through int.TryParse, but there is no need for that. The library provides a convenient typechecking.
```C#

[Command("test")]
public class TestExecutor : CommandExecutor<int, int, bool, string>
{
    public override Message? Execute()
    {
        // This method is called if the user has send 4 arguments. 
        // The first 2 of which were a number, 3 was a bool value, and 4 was a string
    }
}

```

Moreover! In addition to typechecking, the library supports automatic type casting.
You can get the argument with specific type like this:

```C#

var number = Get<int>(0);
var secondNumber = Get<int>(1);
var booleanValue = Get<bool>(2);
var str = Get(3); // or Get<string>(3);

```

### Middlewares
Library supports middlewares.
Ways, which bot passes, before call Execute method. If way returned not false - Execute() will never be called.
This can be used to check if the user is authorized or if the user is an admin or he runs this command from telegrams, etc..

Example:
```C#

public override ExecutorDelegate[] Middlewares { get; } = { (executor => {
    if (executor.User is not TelegramUser) throw new ErrorException("This command supports only telegram");
    return true;
} };


```

### Syntax error and errors
It is inconvenient to write User.Send("Syntax error: /example ...") every time. So.. The library has several exceptions for such situations.
Example:
```C#

throw new SyntaxErrorException("<create/delete>"); // it send for user message "Syntax error. True syntax: /example <create/delete>". Message can be edit in config
throw new ErrorException("Example error"); // it send for user message "Error: Example error"

```

### Keyboards
```C#

var keyboard = new ReplyMarkupKeyboard();

// You can specify button limits per line or per page.
// Their default values - 4 and 6
keyboard.MaxInRows = 2;
keyboard.MaxRows = 4;

keyboard.AddButton("Hello World", () => {
    // This action executing if the user press this button
}, KeyboardColor.Primary); // if the platform supports colors for buttons, then they will be the color, which passed as 3 argument.
keyboard.AddButton("Run /menu command", "/menu", KeyboardColor.Red);

keyboard.AddPage(); // Keyboards in Jubi supported pagination. You can create as many buttons as you like!
keyboard.AddButton("This button in second page. To see me you have to press \"Next page\" button", "/");
keyboard.AddLine(); // Add new line
keyboard.AddButon("I am the button on the second line");

return new Message("Text for user", keyboard); // send message with keyboard
return new Message("This message delete keyboard", new RemoveReplyMarkupKeyboard());
return keyboard; // send keyboard without text


// Also, you can make markup with button "Previous". It is specified in the constructor
// First argument parameter analog "one_time". If true - keyboard hide after press button. Default - false

return new ReplyMarkupKeyboard(false, () => {
// action
});

return new ReplyMarkupKeyboard(false, "/menu");

```

### Attachments
Jubi currently only supports images as attachments. In the future, I will created classes for other attachments.
```C#
return new Message("Photo", new [] { new PhotoAttachment("link to photo, or path") });

return new Message("Photo", new PhotoAttachment(bytes));

return new PhotoAttachment("implicit convert");

```


### Read string
You can get future message from user using Read() or Read<T>()
```C#
var name = User.Read("Send your name");
var age = User.Read<int>("Send your age");
var balance = User.Read<double>("Send your balance");
var isAdmin = User.Read<bool>("You're admin?");

var markup = new ReplyKeyboardMarkup();
markup.AddButton("Item 1", "0");
markup.AddButton("Item 2", "1");

// if user select Item 1 ReadInt return 0, else method return 1
var item = user.ReadInt(new Message("Select item", markup));

var customConvert = user.Read<DateTime>(
    "Send your date of birth",
    new ReadMessageData<DateTime>("Error: Wrong date format", DateTime.TryParse)
);
```

### Subcommands
In Jubi you can make subcommands:

```C#

[Command("test")]
public class TestExecutor : CommandExecutor 
{
    public override CommandExecutor[] Subcommands { get; } = { new SubcommandExecutor(), new Subcommand2Executor() };
}

[Command("subcommand")]
[Ignore]
public class SubcommandExecutor : CommandExecutor 
{
    public override Message? Execute()
    {
        return "Hello world!";
    }
}

[Command("subcommand2")]
[Ignore]
public class Subcommand2Executor : CommandExecutor 
{
    public override Message? Execute()
    {
        return "Hello universe!";
    }
}

```

If user write /test, Jubi send error: "Syntax error! True syntax: /test <subcommand/subcommand2>"
Else if user write /test subcommand - bot send his "Hello world".
Ignore attribute tells reflection that this class should not be registered as command.


### Menu commands

You can make menu-commands:
```C#

[Command("menu")]
public class MenuExecutor : MenuCommandExecutor
{
    public override CommandExecutors[] Subcommands { get; } = { new AdminMenuExecutor() };
}

[Command("Admin menu")]
[Ignore]
public class AdminMenuExecutor : CommandExecutor
{
    public override Message? Execute()
    {
        return "You click to admin button";
    }

}

```

If user write /menu - he receive keyboard menu with one button "Admin menu", who send "You click to admin button"