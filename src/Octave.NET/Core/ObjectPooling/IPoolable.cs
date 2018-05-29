namespace Octave.NET.Core.ObjectPooling
{
    public interface IPoolable
    {
        bool CanBeReused { get; }
    }
}