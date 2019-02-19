using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ria.CalculationEngine.Processors.Interface;

namespace Ria.CalculationEngine.Processors.Implementation
{
    public class ProducerConsumer<T> : IProducerConsumer<T>
    {
        const int MaxBufferSize = 500;
        const int MaxGenerated = 100;
        const int MaxProducers = 2;

        public void AddToProducer(T item)
        {
            var blockingCollection = new BlockingCollection<T>();
            var count = 1;

            var timer = new Timer(new TimerCallback(TimerElapsed), blockingCollection, 0 ,5000);
            Task[] producers = new Task[MaxProducers];
            for (int i = 0; i < MaxProducers; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    while (count <= MaxGenerated)
                    {
                        blockingCollection.Add(item);
                        Interlocked.Increment(ref count);
                        Thread.Sleep(100);
                    }
                });

                producers[i] = task;
            }

            Task.WaitAll(producers);
        }

        /// <summary>
        /// And an action to process the items through the buffer.
        /// </summary>
        public Action<List<T>> ProcessBatchedItems { get; set; }

        private void TimerElapsed(object state)
        {
            var buffer = new List<T>(MaxBufferSize);

            while (((BlockingCollection<T>)state).TryTake(out var item, 0))
            {
                buffer.Add(item);

                if (buffer.Count == MaxBufferSize)
                {
                    break;
                }
            }

            ProcessItems(buffer);
            buffer.Clear();
        }

        private void ProcessItems(List<T> buffer)
        {
            // Now we can process the batched items correctly by firing an action to the caller.
            this.ProcessBatchedItems?.Invoke(buffer);

        }
    }
}
