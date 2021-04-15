using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.ConsoleApp.Executors.Subcommands.Menu;

namespace Jubi.ConsoleApp.Executors
{
    [Command("menu")]
    public class MenuExecutor : MenuCommandExecutor
    {
        public override CommandExecutor[] Subcommands { get; set; } = {new TestExecutor(), new Test2Executor()};
    }
}