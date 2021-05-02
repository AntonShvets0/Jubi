using Jubi.Abstracts.Executors;

namespace Jubi.Exceptions
{
    public class SyntaxErrorException : JubiException
    {
        public string Alias;
        
        public SyntaxErrorException(CommandExecutor executor, string message) : base(message)
        {
            Alias = executor.FullAlias;
        }

        public SyntaxErrorException(string alias, string message) : base(message)
        {
            Alias = alias;
        }
    }
}