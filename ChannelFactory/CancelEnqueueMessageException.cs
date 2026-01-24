using System;

namespace ChannelFactory
{
    public class CancelEnqueueMessageException: Exception
    {
        public CancelEnqueueMessageException(string message)
        : base(message) { }
    }
}