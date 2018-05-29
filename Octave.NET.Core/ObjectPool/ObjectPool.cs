using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Octave.NET.Core.ObjectPool
{
    public interface IObjectPool<T> where T : IPoolable
    {
        T GetObject();
        void ReleaseObject(T obj);
    }

    public class ObjectPool<T> : IObjectPool<T> where T : IPoolable
    {
        private const int ReclaimTaskSleepTimeMs = 2500;
        private const int PoolIdleThresholdMs = 10000;

        private readonly int maxConcurrency = Environment.ProcessorCount;
        private readonly ConcurrentBag<T> internalPool = new ConcurrentBag<T>();
        private readonly Func<T> createObjectFunc;

        private readonly Stopwatch watch = null;

        private CancellationTokenSource reclaimTaskCancellationTokenSource;

        private int 

        ~ObjectPool()
        {
            if (this.reclaimTaskCancellationTokenSource != null)
            {
                this.reclaimTaskCancellationTokenSource.Cancel();
                this.reclaimTaskCancellationTokenSource.Dispose();
            }
        }

        public ObjectPool(Func<T> createObjectFunc, bool reclaimWhenIdle = true, int? maxConcurrency = null)
        {
            if (maxConcurrency != null && maxConcurrency > 0)
                this.maxConcurrency = maxConcurrency.Value;

            this.createObjectFunc = createObjectFunc ?? throw new ArgumentNullException(nameof(createObjectFunc));

            if (reclaimWhenIdle)
            {
                this.watch = new Stopwatch();
                this.watch.Start();

                StartReclaimTask();
            }
        }


        private void StartReclaimTask()
        {
            this.reclaimTaskCancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = this.reclaimTaskCancellationTokenSource.Token;

            Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(ReclaimTaskSleepTimeMs);

                    if (!this.internalPool.IsEmpty && this.watch.ElapsedMilliseconds > PoolIdleThresholdMs)
                    {
                        var success = this.internalPool.TryTake(out var item);

                        if (success)
                        {
                            (item as IDisposable)?.Dispose();
                        }

                        this.watch.Reset();
                    }

                }
            }, cancellationToken);
        }


        public T GetObject()
        {
            this.watch.Reset();
            do
            {
                if (internalPool.TryTake(out var item))
                {
                    if (item.CanBeReused)
                    {
                        return item;
                    }

                    (item as IDisposable)?.Dispose();
                }
            } while ();

            return this.createObjectFunc();
        }

        public void ReleaseObject(T obj)
        {
            this.internalPool.Add(obj);
        }
    }
}
