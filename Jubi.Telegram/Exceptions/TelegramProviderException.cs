using System;

namespace Jubi.Telegram.Exceptions
{
    internal class TelegramProviderException : Exception
    {
        public TelegramProviderException(string message) : base(message)
        {
        }
    }
}