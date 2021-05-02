using System;
using Jubi.Abstracts;
using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.ConsoleApp.Executors.Subcommands;
using Jubi.Response;
using Jubi.Response.Attachments;
using Jubi.Response.Attachments.Keyboard;
using Jubi.Response.Interfaces;

namespace Jubi.ConsoleApp.Executors
{
    [Command("hello")]
    public class HelloExecutor : CommandExecutor
    {
        public override Message? Execute()
        {
            var reply = new ReplyMarkupKeyboard(false, () =>
            {
                User.Send("OK");
            });
            
            User.Read<int>(new Message("Введите int", reply), result =>
            {
                User.Send(result.ToString());
            });

            return null;
        }
    }
}