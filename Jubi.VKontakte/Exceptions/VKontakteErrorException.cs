using System;

namespace Jubi.VKontakte.Exceptions
{
    internal class VKontakteErrorException : Exception
    {
        public int Code;
        public string ErrorMessage;

        public VKontakteErrorException(int code, string message) 
            : base($"{code}: {message}")
        {
            Code = code;
            ErrorMessage = message;
        }
    }
}