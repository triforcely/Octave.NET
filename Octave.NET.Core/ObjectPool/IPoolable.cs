namespace Octave.NET.Core.ObjectPool
{
    public interface IPoolable
    {
        bool CanBeReused { get; }
    }
}
