using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Response;

namespace Jubi.ConsoleApp.Executors
{
    [Command("strict")]
    public class StrictExecutor : CommandExecutor<string, int, bool>
    {
        public override Message? Execute()
        {
            var age = Get<int>(1) + 10;

            return $"Name: {Get(0)}, age: {age}. False? " + (Get<bool>(2) ? "Ok" : "No");
        }
    }
}