using Jubi.Abstracts;
using Jubi.Api;
using Jubi.Console.Api;

namespace Jubi.Console
{
    public class ConsoleProvider : SiteProvider<ConsoleUser>
    {
        public override string Id { get; set; } = "console";

        public override IApiProvider Api { get; set; } = new ConsoleApiProvider();

        public static ConsoleProvider Instance;

        public ConsoleProvider()
        {
            Instance = this;
        }
    }
}