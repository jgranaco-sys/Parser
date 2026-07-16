namespace Parser.ProtocolAdapters.Mqtt;

/// <summary>
/// MQTT adapter configuration.
/// </summary>
public sealed class MqttOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1883;
    public string ClientId { get; set; } = "parser-engine";
    public string IngressProfile { get; set; } = "json-station";
    public string SubscribeTopic { get; set; } = "parser/in";
    public string PublishTopic { get; set; } = "parser/out";
    public string PublishTemplate { get; set; } = "json-out";
}
