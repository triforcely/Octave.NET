using System;

namespace Octave.NET.Core.Exceptions
{
    public class OctaveCommandTimeoutException : TimeoutException
    {
        public OctaveCommandTimeoutException() : base(
            "Execution took too long and will be halted. Operation is taking too long for currently set timeout period or it got stuck on infinite loop.")
        {
        }
    }
}