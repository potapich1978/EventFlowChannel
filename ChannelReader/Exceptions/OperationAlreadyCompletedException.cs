namespace ChannelReader.Exceptions
{
    public sealed class OperationAlreadyCompletedException: OperationException
    {
        public OperationAlreadyCompletedException(string eventName)
            : base($"Operation event '{eventName}' is already completed.")
        { }
    }
    
}