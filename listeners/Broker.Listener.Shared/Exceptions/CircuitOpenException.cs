using System.Runtime.Serialization;

namespace Broker.Listener.Shared.Exceptions;

public class CircuitOpenException : Exception
{
    public CircuitOpenException()
    {
    }

    public CircuitOpenException(string message) : base(message)
    {
    }

    public CircuitOpenException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected CircuitOpenException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
