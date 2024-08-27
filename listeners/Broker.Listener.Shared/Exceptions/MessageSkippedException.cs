using System.Runtime.Serialization;

namespace Broker.Listener.Shared.Exceptions;

public class MessageSkippedException : Exception
{
    public MessageSkippedException()
    {
    }

    public MessageSkippedException(string message) : base(message)
    {
    }

    public MessageSkippedException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected MessageSkippedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
