using System.IO;
using SuperBackendNR85IA.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace SuperBackendNR85IA.Tests;

public class SessionYamlParserTests
{
    [Fact]
    public void ParseSessionInfo_ReturnsData()
    {
        var yaml = File.ReadAllText("../yamls/input_current.yaml");
        var parser = new SessionYamlParser(new NullLogger<SessionYamlParser>());
        var result = parser.ParseSessionInfo(yaml, 0, 0, 1);
        Assert.NotNull(result.Item1);
        Assert.NotNull(result.Item2);
    }
}
