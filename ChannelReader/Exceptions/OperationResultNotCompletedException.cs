namespace ChannelReader.Exceptions
{
    public sealed class OperationResultNotCompletedException: OperationException
    {
        public OperationResultNotCompletedException(string eventName)
            : base($"Operation event '{eventName}' result is not completed yet.")
        { }
    }
}