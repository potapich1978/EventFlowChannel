using System;
using ChannelReader.Exceptions;

namespace ChannelReader.Abstract
{
    public abstract class OperationEvent<TEventType, TResult> : IOperationEvent<TEventType, TResult>
        where TEventType : struct, Enum, IComparable
    {
        private readonly object _syncRoot = new  object();
        private TResult _result;
        private bool _isCompleted;

        public abstract TEventType EventType { get; }

        public bool IsCompleted
        {
            get
            {
                lock (_syncRoot)
                {
                    return _isCompleted;
                }
            }
        }

        public void Complete(TResult result)
        {
            if(result == null)
                throw new ArgumentNullException(nameof(result));

            lock (_syncRoot)
            {
                if (_isCompleted)
                {
                    throw new OperationAlreadyCompletedException(GetType().Name);
                }

                _result = result;
                _isCompleted = true;
            }
        }

        public TResult GetResultOrThrow()
        {
            lock (_syncRoot)
            {
                if (!_isCompleted || _result == null)
                {
                    throw new OperationResultNotCompletedException(GetType().Name);
                }

                return _result;
            }
        }
    }
}