namespace ChannelReader.Exceptions
{
    public sealed class OperationValidationException: OperationException
    {
        public OperationValidationException(string message)
        : base(message) { }
    }
}