using FluentAssertions;
using Vexit.CliEngine.Utils;
using Xunit;

namespace Vexit.CliEngine.Tests.Utils;

public class CliUtilTests
{
    [Fact]
    public void ParseCommandLine_Returns_Empty_For_EmptyInput()
    {
        var result = CliUtil.ParseCommandLine(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public void ParseCommandLine_Splits_Unquoted_Arguments()
    {
        var result = CliUtil.ParseCommandLine("vexit server setup");

        result.Should().Equal("vexit", "server", "setup");
    }

    [Fact]
    public void ParseCommandLine_Respects_DoubleQuoted_Arguments()
    {
        var result = CliUtil.ParseCommandLine("deploy --name \"my app\" --env prod");

        result.Should().Equal("deploy", "--name", "my app", "--env", "prod");
    }

    [Fact]
    public void ParseCommandLine_Respects_SingleQuoted_Arguments()
    {
        var result = CliUtil.ParseCommandLine("deploy --name 'my app' --env prod");

        result.Should().Equal("deploy", "--name", "my app", "--env", "prod");
    }
}
