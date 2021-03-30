using System;

namespace Jubi.Exceptions
{ 
    public class JubiException : Exception
    {
        public JubiException(string message) : base(message)
        {
        }
    }
}