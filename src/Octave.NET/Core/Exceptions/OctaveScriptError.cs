using System;

namespace Octave.NET.Core.Exceptions
{
    public class OctaveScriptError : Exception
    {
        public OctaveScriptError(string message) : base(message)
        {
        }

        public OctaveScriptError(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}