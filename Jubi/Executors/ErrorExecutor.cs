using System;
using Jubi.Abstracts;
using Jubi.Abstracts.Executors;
using Jubi.Attributes;
using Jubi.Response;

namespace Jubi.Executors
{
    [Command("error")]
    public class ErrorExecutor : CommandExecutor<string>
    {
        public override Message? Execute()
        {
            switch (Get<string>(0))
            {
                case "unknown_command": return UnknownCommandError();
                case "exception": return ExceptionError();
            }

            return null;
        }

        public virtual Message? UnknownCommandError()
            => BotInstance.Configuration["error"]["unknown_command"].ToString();

        public virtual Message? ExceptionError()
            => BotInstance.Configuration["error"]["internal_error"].ToString();
    }
}