using Octave.NET.Core.ObjectPooling;
using System.Diagnostics;

namespace Octave.NET
{
    internal class OctaveProcess : Process, IPoolable
    {
        public event DataEventHandler OnData;
        public event DataEventHandler OnError;

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

            this.OutputDataReceived += OctaveProcess_OutputDataReceived;
            this.ErrorDataReceived += OctaveProcess_ErrorDataReceived;

            Start();

            this.BeginErrorReadLine();
            this.BeginOutputReadLine();

            this.StandardInput.AutoFlush = false;
        }

        public void Write(string data)
        {
            this.StandardInput.WriteLine(data);
            this.StandardInput.Flush();
        }

        private void OctaveProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnError?.Invoke(sender, e.Data);
        }

        private void OctaveProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnData?.Invoke(sender, e.Data);
        }

        public bool CanBeReused => !isDisposed && !HasExited;

        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            if (!HasExited) Kill();

            base.Dispose(disposing);
        }
    }

    internal delegate void DataEventHandler(object sender, string data);
}