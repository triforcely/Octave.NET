using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Octave.NET.Core.ObjectPooling
{
    internal interface IObjectPool<T> where T : IPoolable
    {
        T GetObject();
        void ReleaseObject(T obj);
    }

    internal class ObjectPool<T> : IObjectPool<T> where T : IPoolable
    {
        private readonly Func<T> createObjectFunc;

        private readonly ConcurrentBag<T> internalPool = new ConcurrentBag<T>();
        private readonly int MaxConcurrency = Environment.ProcessorCount;
        private readonly int PoolIdleThresholdMs = 2500;
        private readonly int ReclaimTaskSleepTimeMs;
        private readonly Stopwatch watch;

        private int aliveObjects;

        private CancellationTokenSource reclaimTaskCancellationTokenSource;

        public ObjectPool(Func<T> createObjectFunc, int reclaimTaskSleepTime, bool reclaimWhenIdle = true,
            int? maxConcurrency = null)
        {
            ReclaimTaskSleepTimeMs = reclaimTaskSleepTime;

            if (maxConcurrency != null && maxConcurrency > 0)
                MaxConcurrency = maxConcurrency.Value;

            this.createObjectFunc = createObjectFunc ?? throw new ArgumentNullException(nameof(createObjectFunc));

            if (!reclaimWhenIdle) return;

            watch = new Stopwatch();
            watch.Start();

            StartReclaimTask();
        }


        public T GetObject()
        {
            lock (createObjectFunc)
            {
                watch?.Restart();

                var limitReached = false;

                do
                {
                    if (internalPool.TryTake(out var item))
                    {
                        if (item.CanBeReused) return item;

                        (item as IDisposable)?.Dispose();
                        aliveObjects--;
                    }

                    limitReached = aliveObjects >= MaxConcurrency;

                    if (limitReached)
                        Thread.Sleep(1);
                } while (limitReached);

                aliveObjects++;
                return createObjectFunc();
            }
        }

        public void ReleaseObject(T obj)
        {
            internalPool.Add(obj);
        }

        ~ObjectPool()
        {
            foreach (var item in internalPool)
                (item as IDisposable)?.Dispose();

            if (reclaimTaskCancellationTokenSource == null) return;

            reclaimTaskCancellationTokenSource.Cancel();
            reclaimTaskCancellationTokenSource.Dispose();
        }


        private void StartReclaimTask()
        {
            reclaimTaskCancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = reclaimTaskCancellationTokenSource.Token;

            Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(ReclaimTaskSleepTimeMs);

                    if (internalPool.IsEmpty || watch.ElapsedMilliseconds <= PoolIdleThresholdMs) continue;

                    var success = internalPool.TryTake(out var item);

                    if (success)
                    {
                        (item as IDisposable)?.Dispose();
                        aliveObjects--;
                    }


                    watch?.Restart();
                }
            }, cancellationToken);
        }
    }
}