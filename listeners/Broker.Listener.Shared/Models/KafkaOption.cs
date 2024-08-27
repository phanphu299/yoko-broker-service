using Confluent.Kafka;

namespace Broker.Listener.Shared.Models;

public class KafkaOption
{
    public string BootstrapServers { get; set; }
    public Acks? AckMode { get; set; }
    public double? Linger { get; set; }
    public int? BatchSize { get; set; }
}
