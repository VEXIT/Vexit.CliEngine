using FluentAssertions;
using Vexit.CliEngine.Attributes;
using Vexit.CliEngine.BaseClasses;
using Xunit;

namespace Vexit.CliEngine.Tests;

public class CliParserTests
{
    [Fact]
    public void Parse_Binds_Positionals_Options_And_Flags()
    {
        var parser = new CliParser();
        var cmd = new DeployCommand();

        var parsed = parser.Parse(cmd, ["api", "--port", "8080", "-f", "--tag", "blue", "--tag", "green"]);
        parsed.ApplyToCommand(cmd);

        parsed.HasValidationErrors.Should().BeFalse();
        cmd.Name.Should().Be("api");
        cmd.Port.Should().Be(8080);
        cmd.Force.Should().BeTrue();
        cmd.Tags.Should().Equal("blue", "green");
    }

    [Fact]
    public void Parse_Allows_Negative_Number_As_Positional_Value()
    {
        var parser = new CliParser();
        var cmd = new OffsetCommand();

        var parsed = parser.Parse(cmd, ["-5"]);
        parsed.ApplyToCommand(cmd);

        parsed.HasValidationErrors.Should().BeFalse();
        cmd.Offset.Should().Be(-5);
    }

    [Fact]
    public void Parse_Adds_Error_For_Invalid_Dashed_Positional_Format()
    {
        var parser = new CliParser();
        var cmd = new NameOnlyCommand();

        var parsed = parser.Parse(cmd, ["-invalid-token"]);

        parsed.HasValidationErrors.Should().BeTrue();
        parsed.ValidationErrors.Should().ContainSingle(e => e.Contains("not a valid argument format"));
    }

    [Fact]
    public void Parse_Adds_Error_For_Unknown_Option()
    {
        var parser = new CliParser();
        var cmd = new NameOnlyCommand();

        var parsed = parser.Parse(cmd, ["--mystery"]);

        parsed.HasValidationErrors.Should().BeTrue();
        parsed.ValidationErrors.Should().Contain("Unknown option: mystery");
    }

    [Fact]
    public void Parse_Adds_Error_When_Required_Option_Is_Missing()
    {
        var parser = new CliParser();
        var cmd = new RequiredOptionCommand();

        var parsed = parser.Parse(cmd, []);

        parsed.HasValidationErrors.Should().BeTrue();
        parsed.ValidationErrors.Should().Contain("--port is required.");
    }

    [Fact]
    public void Parse_NullableBooleanTriState_OptIn_BareFlag_Sets_True()
    {
        var parser = new CliParser();
        var cmd = new OptInCommand();

        var parsed = parser.Parse(cmd, ["--opt-in"]);
        parsed.ApplyToCommand(cmd);

        parsed.HasValidationErrors.Should().BeFalse();
        cmd.OptIn.Should().BeTrue();
    }

    [Fact]
    public void Parse_NullableBooleanTriState_OptIn_EqualsFalse_Sets_False()
    {
        var parser = new CliParser();
        var cmd = new OptInCommand();

        var parsed = parser.Parse(cmd, ["--opt-in=false"]);
        parsed.ApplyToCommand(cmd);

        parsed.HasValidationErrors.Should().BeFalse();
        cmd.OptIn.Should().BeFalse();
    }

    [Fact]
    public void Parse_NullableBooleanTriState_OptIn_Omitted_Stays_Null()
    {
        var parser = new CliParser();
        var cmd = new OptInCommand();

        var parsed = parser.Parse(cmd, []);
        parsed.ApplyToCommand(cmd);

        parsed.HasValidationErrors.Should().BeFalse();
        cmd.OptIn.Should().BeNull();
    }

    private sealed class DeployCommand : CmdBase
    {
        [Argument("name", isRequired: true)]
        public string Name { get; set; } = string.Empty;

        [Option("port", "p", isRequired: true)]
        public int Port { get; set; }

        [Option("force", "f")]
        public bool Force { get; set; }

        [Option("tag", "t")]
        public List<string> Tags { get; set; } = [];
    }

    private sealed class OffsetCommand : CmdBase
    {
        [Argument("offset", isRequired: true)]
        public int Offset { get; set; }
    }

    private sealed class NameOnlyCommand : CmdBase
    {
        [Argument("name", isRequired: false)]
        public string? Name { get; set; }
    }

    private sealed class RequiredOptionCommand : CmdBase
    {
        [Option("port", isRequired: true)]
        public int Port { get; set; }
    }

    private sealed class OptInCommand : CmdBase
    {
        [Option("opt-in", description: "Tri-state opt-in flag")]
        public bool? OptIn { get; set; }
    }
}
