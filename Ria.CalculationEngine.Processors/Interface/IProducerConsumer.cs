using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ria.CalculationEngine.Processors.Interface
{
    public interface IProducerConsumer<T>
    {
        void Start(BlockingCollection<T> items);
        Action<IEnumerable<T>> ProcessBatchedItems { get; set; }
       
    }
}
