using Octave.NET.Core.ObjectPooling;
using System.Diagnostics;

namespace Octave.NET
{
    internal class OctaveProcess : Process, IPoolable
    {
        private readonly bool isDisposed = false;

        public OctaveProcess(string octaveCliPath)
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = octaveCliPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Start();

            BeginOutputReadLine();
            BeginErrorReadLine();

            this.StandardInput.AutoFlush = false;
        }

        public bool CanBeReused => !isDisposed && !HasExited;

        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (!HasExited) Kill();

            base.Dispose(disposing);
        }
    }
}