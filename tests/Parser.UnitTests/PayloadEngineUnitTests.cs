using Parser.Abstractions;
using Parser.Core.Mapping;
using Parser.Core.Template;
using Parser.Core.Transformations;
using Parser.Core.Validation;
using Parser.Plugins.BuiltIn;
using Xunit;

namespace Parser.UnitTests;

public sealed class PayloadEngineUnitTests
{
    [Fact]
    public async Task JsonParser_FlattensSimpleDocument()
    {
        var parser = new JsonPayloadParser();
        var envelope = new PayloadEnvelope(
            "topic",
            null,
            System.Text.Encoding.UTF8.GetBytes("{" + "\"StationA\":{\"Voltage\":138.2,\"Breaker\":\"OPEN\",\"Temperature\":25.3}}"),
            "json");

        var document = await parser.ParseAsync(envelope);

        Assert.Equal(138.2d, document.Values["StationA.Voltage"]);
        Assert.Equal("OPEN", document.Values["StationA.Breaker"]);
        Assert.Equal(25.3d, document.Values["StationA.Temperature"]);
    }

    [Fact]
    public void MappingEngine_ExpandsWildcardCaptures()
    {
        var engine = new MappingEngine();
        var document = new CanonicalDocument(new Dictionary<string, object?>
        {
            ["measurements[0].voltage"] = 13.8,
            ["measurements[1].voltage"] = 14.1
        });
        var profile = new MappingProfile(
            "wildcard",
            "json",
            new[] { new MappingRule("measurements[*].voltage", "BUS{0}_V") });

        var mapped = engine.Map(document, profile);

        Assert.Contains(mapped, point => point.TargetVariable == "BUS0_V" && Equals(point.Value, 13.8));
        Assert.Contains(mapped, point => point.TargetVariable == "BUS1_V" && Equals(point.Value, 14.1));
    }

    [Fact]
    public void TransformationEngine_AppliesEnumAndExpressionRules()
    {
        var engine = new TransformationEngine();
        var points = new[]
        {
            new MappedPoint("StationA.Voltage", "BUS1_V", 138.2, false),
            new MappedPoint("StationA.Breaker", "BRK1_ST", "OPEN", false)
        };
        var profile = new TransformationProfile(
            "transform",
            new[]
            {
                new TransformationRule("BUS1_V", new[]
                {
                    new TransformationStep("EXPRESSION", "IncomingValue * 1000")
                }),
                new TransformationRule("BRK1_ST", new[]
                {
                    new TransformationStep("ENUM", Map: new Dictionary<string, string> { ["OPEN"] = "1" }),
                    new TransformationStep("CONVERT", "int")
                })
            });

        var transformed = engine.Transform(points, profile);

        Assert.Equal(138200d, transformed.Single(item => item.Name == "BUS1_V").Value.Value);
        Assert.Equal(1, transformed.Single(item => item.Name == "BRK1_ST").Value.Value);
    }

    [Fact]
    public void TemplateRenderer_RendersFormatsAndConditionals()
    {
        var renderer = new TemplateRenderer();
        var values = new Dictionary<string, ScadaValue>
        {
            ["BUS1_V"] = new(138200d, "double", DateTimeOffset.UtcNow),
            ["BRK1_ST"] = new(1, "int", DateTimeOffset.UtcNow),
            ["Timestamp"] = new(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), "datetimeoffset", DateTimeOffset.UtcNow)
        };

        var rendered = renderer.Render("Voltage=${BUS1_V:F2};Breaker=${BRK1_ST==\"1\" ? 1 : 0};At=${Timestamp:yyyy-MM-dd HH:mm:ss}", values);

        Assert.Equal("Voltage=138200.00;Breaker=1;At=2026-01-02 03:04:05", rendered);
        Assert.Contains("BUS1_V", renderer.DiscoverVariables("${BUS1_V:F2}${BRK1_ST==\"1\" ? 1 : 0}"));
    }

    [Fact]
    public void ValidationEngine_RejectsOutOfRangeValues()
    {
        var engine = new ValidationEngine();
        var points = new[]
        {
            new ScadaWriteRequest("TEMP1", new ScadaValue(999d, "double", DateTimeOffset.UtcNow), "StationA.Temperature")
        };
        var profile = new ValidationProfile(
            "validation",
            PartialUpdateMode.AcceptValidOnly,
            new[] { new ValidationRule("TEMP1", Required: true, ExpectedType: "double", Minimum: -50, Maximum: 200) });

        var result = engine.Validate(points, profile, DateTimeOffset.UtcNow);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Points, point => point.VariableName == "TEMP1" && !point.Accepted);
    }
}
