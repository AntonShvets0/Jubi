using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Response;

namespace Jubi.ConsoleApp.Executors.Subcommands
{
    [Command("hello2")]
    [Ignore]
    public class Hello2Executor : CommandExecutor
    {
        public override Message? Execute()
        {
            return "Ohayo";
        }
    }
}