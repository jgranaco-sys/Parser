namespace Parser.ProtocolAdapters.Mqtt;

/// <summary>
/// Thin MQTT adapter that bridges messages into the ingress pipeline and publishes rendered output.
/// </summary>
public sealed class MqttAdapterHostedService(
    IOptions<MqttOptions> options,
    IIngressPipeline ingressPipeline,
    IEgressPipeline egressPipeline,
    ILogger<MqttAdapterHostedService> logger) : IHostedService, IDisposable
{
    private IMqttClient? _client;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("MQTT adapter is disabled.");
            return;
        }

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += async args =>
        {
            var message = args.ApplicationMessage;
            var payload = message.PayloadSegment.ToArray();
            var headers = message.UserProperties?.ToDictionary(property => property.Name, property => property.Value)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await ingressPipeline.ProcessAsync(options.Value.IngressProfile, new PayloadEnvelope(
                message.Topic,
                headers,
                payload,
                FormatHint: "json",
                MessageId: Guid.NewGuid().ToString("N"),
                CorrelationId: Guid.NewGuid().ToString("N")), cancellationToken);
        };

        var mqttOptions = new MqttClientOptionsBuilder()
            .WithClientId(options.Value.ClientId)
            .WithTcpServer(options.Value.Host, options.Value.Port)
            .Build();
        await _client.ConnectAsync(mqttOptions, cancellationToken);
        await _client.SubscribeAsync(options.Value.SubscribeTopic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, cancellationToken);
        logger.LogInformation("MQTT adapter subscribed to {Topic}.", options.Value.SubscribeTopic);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client is not null)
        {
            if (_client.IsConnected)
            {
                var rendered = await egressPipeline.GenerateAsync(options.Value.PublishTemplate, cancellationToken);
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(options.Value.PublishTopic)
                    .WithPayload(rendered.PayloadBytes)
                    .Build();
                await _client.PublishAsync(message, cancellationToken);
                await _client.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
            }

            _client.Dispose();
            _client = null;
        }
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}
