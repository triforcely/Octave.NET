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
    public partial class OctaveHost : IDisposable
    {
        private const int CommandTimeoutMilliseconds = 30000;
        private readonly string finishToken = Guid.NewGuid().ToString();
        private readonly AutoResetEvent manualResetEvent = new AutoResetEvent(false);
        private string lastError = string.Empty;
        private OctaveProcess workerProcess;

        public OctaveHost()
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

                if (e.Data.Contains(localFinishToken))
                {
                    data.Length -= Environment.NewLine.Length;

                    var finishTokenAnsLenght = "ans = ".Length + localFinishToken.Length;

                    data.Length -= Math.Min(data.Length, finishTokenAnsLenght);
                    localMre.Set();
                }
            }

            void LocalOnError(object sender, DataReceivedEventArgs e)
            {
                if (e?.Data == null) return;

                hasError = true;

                error.Append(e.Data + Environment.NewLine);

                localMre.Set();
            }

            workerProcess.OutputDataReceived += LocalOnData;
            workerProcess.ErrorDataReceived += LocalOnError;

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

            workerProcess.OutputDataReceived -= LocalOnData;
            workerProcess.ErrorDataReceived -= LocalOnError;

            if (!isDone) throw new OctaveCommandTimeoutException();

            var response = data.ToString().Trim();
            return response;
        }

        /// <summary>
        ///     Execute multiple payloads with octave command(s), e.g. when script takes input using octave's "input(...)"
        ///     function.
        /// </summary>
        /// <param name="commands">Collection with octave commands.</param>
        /// <param name="ignoreScriptErrors">Whether errors in scripts should result in exception</param>
        /// <param name="timeout">Command timeout in milliseconds</param>
        /// <exception cref="OctaveCommandTimeoutException"></exception>
        /// <exception cref="OctaveScriptError"></exception>
        public void ExecuteMultiple(IEnumerable<string> commands, bool ignoreScriptErrors = false,
            int timeout = CommandTimeoutMilliseconds)
        {
            if (!commands.Any())
                return;

            PrepareExecution();

            foreach (var command in commands)
                workerProcess.StandardInput.WriteLine($"{command};");

            var isComplete = FinishExecute(ignoreScriptErrors);

            if (isComplete) return;

            CommandTimeout();
        }


        public void Dispose()
        {
            if (workerProcess == null) return;

            UnloadWorkerProcess();
        }

        ~OctaveHost()
        {
            Dispose();
        }

        private void Initialize()
        {
            workerProcess = processPool.GetObject();

            workerProcess.StandardInput.WriteLine("more off;");
            workerProcess.StandardInput.WriteLine("split_long_rows(0);");

            workerProcess.OutputDataReceived += OnData;
            workerProcess.ErrorDataReceived += OnErrorData;

            workerProcess.StandardInput.AutoFlush = false;
        }

        private void UnloadWorkerProcess()
        {
            workerProcess.OutputDataReceived -= OnData;
            workerProcess.ErrorDataReceived -= OnErrorData;

            processPool.ReleaseObject(workerProcess);
            workerProcess = null;
        }

        private void OnErrorData(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null) return;

            lastError += e.Data + Environment.NewLine;
        }

        private void OnData(object sender, DataReceivedEventArgs e)
        {
            if (e?.Data == null)
                return;

            if (e.Data.Contains(finishToken))
                manualResetEvent.Set();
        }

        private void PrepareExecution()
        {
            lastError = string.Empty;
            manualResetEvent.Reset();
        }

        private bool FinishExecute(bool ignoreScriptErrors)
        {
            workerProcess.StandardInput.WriteLine($"\"{finishToken}\"");
            workerProcess.StandardInput.Flush();

            var isComplete = manualResetEvent.WaitOne(CommandTimeoutMilliseconds);

            if (!ignoreScriptErrors && !string.IsNullOrEmpty(lastError)) throw new OctaveScriptError(lastError);

            return isComplete;
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