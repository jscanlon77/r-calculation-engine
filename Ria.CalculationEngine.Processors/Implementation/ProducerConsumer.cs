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
        const int BufferSize = 20;
        public void Start(BlockingCollection<T> item)
        {
            int initialCount = item.Count;
            int totalProcessed = 0;
            Task.Factory.StartNew(() =>
            {
                var buffer = new List<T>(MaxBufferSize);

                foreach (var value in item.GetConsumingEnumerable())
                {
                    buffer.Add(value);

                    
                    if (buffer.Count == BufferSize)
                    {
                        totalProcessed += buffer.Count;
                        ProcessItems(buffer);
                        buffer.Clear();
                    }

                    // process the last remaining items which are smaller than the batch size.
                    if (initialCount - totalProcessed < BufferSize)
                    {
                        var items = item.Take(initialCount - totalProcessed).ToList();
                        ProcessItems(items.ToList());
                        buffer.Clear();
                        break;
                    }
                }
            });

        }

        /// <summary>
        /// And an action to process the items through the buffer.
        /// </summary>
        public Action<IEnumerable<T>> ProcessBatchedItems { get; set; }

        
        private void ProcessItems(IEnumerable<T> buffer)
        {
            // Now we can process the batched items correctly by firing an action to the caller.
            this.ProcessBatchedItems?.Invoke(buffer);

        }
    }
}
