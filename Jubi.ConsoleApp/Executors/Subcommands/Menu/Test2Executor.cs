using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Response;

namespace Jubi.ConsoleApp.Executors.Subcommands.Menu
{
    [Command("Второй вариант")]
    [Ignore]
    public class Test2Executor : CommandExecutor
    {
        public override Message? Execute()
        {
            return "Test2!";
        }
    }
}