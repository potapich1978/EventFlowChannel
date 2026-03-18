using System;

namespace ChannelReader.Exceptions
{
    public abstract class OperationException : Exception
    {
        protected OperationException(string message)
        : base(message) {}
    }
}