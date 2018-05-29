using System;

namespace Octave.NET.Core.Exceptions
{
    public class OctaveScriptError : Exception
    {
        public OctaveScriptError(string message) : base(message)
        {
        }
    }
}