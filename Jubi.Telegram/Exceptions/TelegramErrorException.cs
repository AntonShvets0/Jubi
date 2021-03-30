using System;

namespace Jubi.Telegram.Exceptions
{
    internal class TelegramErrorException : Exception
    {
        public int Code;
        public string ErrorMessage;

        public TelegramErrorException(int code, string message) 
            : base($"{code}: {message}")
        {
            Code = code;
            ErrorMessage = message;
        }
    }
}