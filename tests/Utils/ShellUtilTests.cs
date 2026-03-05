using FluentAssertions;
using Vexit.CliEngine.Utils;
using Xunit;

namespace Vexit.CliEngine.Tests.Utils;

public class ShellUtilTests
{
    [Fact]
    public void GetProfile_Returns_Failure_When_ShellName_Missing()
    {
        var result = ShellUtil.GetProfile(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Message.Should().Be("Shell name is required");
    }

    [Fact]
    public void GetProfile_Returns_Zsh_Profile()
    {
        var result = ShellUtil.GetProfile("zsh");

        result.IsSuccess.Should().BeTrue();
        result.Data.Shell.Should().Be("zsh");
        result.Data.FilePath.Should().Be(".zshrc");
    }

    [Fact]
    public void GetProfile_Returns_Fish_Profile()
    {
        var result = ShellUtil.GetProfile("fish");

        result.IsSuccess.Should().BeTrue();
        result.Data.Shell.Should().Be("fish");
        result.Data.FilePath.Should().Be(".config/fish/config.fish");
    }

    [Theory]
    [InlineData("bash", true)]
    [InlineData("zsh", true)]
    [InlineData("pwsh", false)]
    [InlineData("", false)]
    public void IsShellSupported_Returns_Expected_Result(string shellName, bool expected)
    {
        var result = ShellUtil.IsShellSupported(shellName);

        result.Should().Be(expected);
    }
}
