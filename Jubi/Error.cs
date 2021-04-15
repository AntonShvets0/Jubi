namespace Jubi
{
    public class Error
    {
        public static string FromConfig(Bot bot, string key)
            => bot.Configuration["errors"][key];
    }
}