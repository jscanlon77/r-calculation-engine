using System;
using System.Collections.Generic;

namespace Ria.CalculationEngine.Processors.Interface
{
    public interface IProducerConsumer<T>
    {
        void AddToProducer(T item);
        Action<List<T>> ProcessBatchedItems { get; set; }
    }
}
