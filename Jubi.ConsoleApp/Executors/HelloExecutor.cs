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
        public override CommandExecutor[] Subcommands { get; set; } = { new Hello2Executor() };
    }
}