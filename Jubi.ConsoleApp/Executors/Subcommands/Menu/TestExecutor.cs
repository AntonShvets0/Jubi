using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Response;

namespace Jubi.ConsoleApp.Executors.Subcommands.Menu
{
    [Command("Первый вариант")]
    [Ignore]
    public class TestExecutor : CommandExecutor
    {
        public override Message? Execute()
        {
            return "Test!";
        }
    }
}