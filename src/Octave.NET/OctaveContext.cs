using Octave.NET.Core.Exceptions;
using Octave.NET.Core.ObjectPooling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Octave.NET
{
    /// <summary>
    /// Octave execution context.
    /// </summary>
    public partial class OctaveContext : IDisposable
    {
        private const int CommandTimeoutMilliseconds = 30000;
        private OctaveProcess workerProcess;

        public OctaveContext()
        {
            lock (poolCreateLock)
            {
                if (processPool == null)
                {
                    var path = OctaveSettings.OctaveCliPath ?? OctaveExecutable;

                    processPool = new ObjectPool<OctaveProcess>(() => new OctaveProcess(path),
                        OctaveSettings.RemoveIdleWorkersTimePeriodMs, !OctaveSettings.PreventColdStarts,
                        OctaveSettings.MaximumConcurrency);
                }

                Initialize();
            }
        }

        /// <summary>
        ///     Execute series of commands and return combined response.
        /// </summary>
        /// <param name="commands">Collection with octave commands.</param>
        /// <param name="timeout">Command timeout in milliseconds</param>
        /// <exception cref="OctaveCommandTimeoutException"></exception>
        /// <exception cref="OctaveScriptError"></exception>
        public string ExecuteMultiple(IEnumerable<string> commands, int timeout = CommandTimeoutMilliseconds)
        {
            if (!commands.Any())
                return "";

            var localFinishToken = Guid.NewGuid().ToString();
            var hasError = false;

            var data = new StringBuilder();
            var error = new StringBuilder();

            var localMre = new AutoResetEvent(false);

            void LocalOnData(object sender, DataReceivedEventArgs e)
            {
                if (e?.Data == null) return;

                data.Append(e.Data);
                data.Append(Environment.NewLine);

                if (!e.Data.Contains(localFinishToken)) return;

                data.Length -= Environment.NewLine.Length;
                var finishTokenAnsLenght = "ans = ".Length + localFinishToken.Length;

                data.Length -= Math.Min(data.Length, finishTokenAnsLenght);
                localMre.Set();
            }

            void LocalOnError(object sender, DataReceivedEventArgs e)
            {
                if (e?.Data == null) return;

                hasError = true;

                error.Append(e.Data + Environment.NewLine);

                localMre.Set();
            }

            try
            {
                workerProcess.OutputDataReceived += LocalOnData;
                workerProcess.ErrorDataReceived += LocalOnError;

                foreach (var command in commands)
                    workerProcess.StandardInput.WriteLine(command);

                workerProcess.StandardInput.WriteLine($"\"{localFinishToken}\"");
                workerProcess.StandardInput.Flush();

                var isDone = localMre.WaitOne(timeout);

                if (hasError)
                {
                    Thread.Sleep(100); // wait so complete error is passed to event handler
                    throw new OctaveScriptError(error.ToString().Trim());
                }

                error.Length -= Math.Min(error.Length, Environment.NewLine.Length);

                if (!isDone) CommandTimeout();

                var response = data.ToString().Trim();
                return response;
            }
            finally
            {
                workerProcess.OutputDataReceived -= LocalOnData;
                workerProcess.ErrorDataReceived -= LocalOnError;
            }
        }

        /// <summary>
        ///     Execute command and return raw response.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="timeout"></param>
        /// <exception cref="OctaveCommandTimeoutException"></exception>
        /// <exception cref="OctaveScriptError"></exception>
        public string Execute(string command, int timeout = CommandTimeoutMilliseconds)
        {
            return this.ExecuteMultiple(new string[] { command }, timeout);
        }

        public void Dispose()
        {
            if (workerProcess == null) return;

            UnloadWorkerProcess();
        }

        ~OctaveContext()
        {
            Dispose();
        }

        private void Initialize()
        {
            workerProcess = processPool.GetObject();

            workerProcess.StandardInput.WriteLine("more off;");
            workerProcess.StandardInput.WriteLine("split_long_rows(0);");
        }

        private void UnloadWorkerProcess()
        {
            processPool.ReleaseObject(workerProcess);
            workerProcess = null;
        }

        private void CommandTimeout()
        {
            //TODO find crossplatform way of doing CTRL+C/equivalent instead of throwing away process.
            workerProcess.Kill(); // object pool will take care of it since it cannot be reused
            UnloadWorkerProcess();
            Initialize();

            throw new OctaveCommandTimeoutException();
        }
    }
}