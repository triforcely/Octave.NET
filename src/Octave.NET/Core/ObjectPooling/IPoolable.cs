namespace Octave.NET.Core.ObjectPooling
{
    internal interface IPoolable
    {
        bool CanBeReused { get; }
    }
}