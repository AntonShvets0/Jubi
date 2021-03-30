using System;

namespace Jubi.VKontakte.Exceptions
{
    internal class VKontakteErrorException : Exception
    {
        public int Code;
        public string Message;

        public VKontakteErrorException(int code, string message) 
            : base($"{code}: {message}")
        {
            Code = code;
            Message = message;
        }
    }
}