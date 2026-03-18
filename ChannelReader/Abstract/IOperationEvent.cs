using System;

namespace ChannelReader.Abstract
{
    public interface IOperationEvent<TEventType, TResult> : IGenericEvent<TEventType>
        where TEventType : struct, Enum, IComparable
    {
        bool IsCompleted { get; }

        void Complete(TResult result);

        TResult GetResultOrThrow();
    }
}