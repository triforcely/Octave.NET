using System;
using Octave.NET.Core.ObjectPooling;

namespace Octave.NET
{
    partial class OctaveContext
    {
        private const string OctaveExecutable = "octave-cli";
        private static readonly object poolCreateLock = new object();
        private static IObjectPool<OctaveProcess> processPool;
        private static OctaveSettings _octaveSettings = new OctaveSettings();

        public static OctaveSettings OctaveSettings
        {
            get => _octaveSettings;
            set
            {
                if (processPool != null)
                    throw new InvalidOperationException(
                        $"Can't change global settings when atleast one {nameof(OctaveContext)} instance was created.");

                _octaveSettings = value;
            }
        }
    }
}