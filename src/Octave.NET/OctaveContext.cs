using Octave.NET.Core.Exceptions;
using Octave.NET.Core.ObjectPooling;
using System;
using System.Text;
using System.Threading;

namespace Octave.NET
{
    public interface IOctaveContext
    {
        string Execute(string command, int timeout);
    }

    /// <summary>
    /// Octave execution context.
    /// </summary>
    public partial class OctaveContext : IDisposable, IOctaveContext
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
            if (string.IsNullOrEmpty(command))
                return "";

            if (this.workerProcess == null)
                Initialize();

            var localFinishToken = Guid.NewGuid().ToString();
            var hasError = false;

            var data = new StringBuilder();
            var error = new StringBuilder();

            var localMre = new AutoResetEvent(false);

            DataEventHandler LocalOnData = (object sender, string dataStr) =>
            {
                if (dataStr == null) return;

                data.Append(dataStr);
                data.Append(Environment.NewLine);

                if (!dataStr.Contains(localFinishToken)) return;

                data.Length -= Environment.NewLine.Length;
                var finishTokenAnsLenght = "ans = ".Length + localFinishToken.Length;

                data.Length -= Math.Min(data.Length, finishTokenAnsLenght);
                localMre.Set();
            };

            DataEventHandler LocalOnError = (object sender, string errorStr) =>
            {
                if (errorStr == null) return;

                hasError = true;

                error.Append(errorStr + Environment.NewLine);

                localMre.Set();
            };

            try
            {
                workerProcess.OnData += LocalOnData;
                workerProcess.OnError += LocalOnError;

                workerProcess.Write(command);
                workerProcess.Write($"\"{localFinishToken}\"");

                var isDone = localMre.WaitOne(timeout);

                if (hasError)
                {
                    throw new System.IO.IOException();
                }

                error.Length -= Math.Min(error.Length, Environment.NewLine.Length);

                if (!isDone) CommandTimeout();

                var response = data.ToString().Trim();
                return response;
            }
            catch (System.IO.IOException exception)
            {
                Thread.Sleep(25); // wait in case collected data is incomplete

                workerProcess.OnData -= LocalOnData;
                workerProcess.OnError -= LocalOnError;

                UnloadWorkerProcess();

                throw new OctaveScriptError(error.ToString(), exception);
            }
            finally
            {
                if (workerProcess != null)
                {
                    workerProcess.OnData -= LocalOnData;
                    workerProcess.OnError -= LocalOnError;
                }
            }
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
            if (!workerProcess.HasExited)
                workerProcess.Kill(); // object pool will take care of it since it cannot be reused

            UnloadWorkerProcess();

            throw new OctaveCommandTimeoutException();
        }
    }
}