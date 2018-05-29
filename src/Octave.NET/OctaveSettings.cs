using System;

namespace Octave.NET
{
    public class OctaveSettings
    {
        /// <summary>
        ///     Maximum number of octave processes which can be simultaneously spawned. Defaults to the number of processors.
        /// </summary>
        public int? MaximumConcurrency { get; set; } = Environment.ProcessorCount;

        /// <summary>
        ///     Path to the octave-cli.exe binary. Can be automatically detected if added to PATH variable.
        /// </summary>
        public string OctaveCliPath { get; set; }

        /// <summary>
        ///     When set to true, octave processes will never be killed even if they are idle for very long time. Defaults to
        ///     false.
        /// </summary>
        public bool PreventColdStarts { get; set; } = false;

        /// <summary>
        ///     How often should it check for idle workers and dispose them. Defaults to 10000.
        /// </summary>
        public int RemoveIdleWorkersTimePeriodMs { get; set; } = 10000;
    }
}