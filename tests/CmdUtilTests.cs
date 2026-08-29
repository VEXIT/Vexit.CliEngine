using FluentAssertions;
using Vexit.CliEngine.Utils;
using Xunit;

namespace Vexit.CliEngine.Tests;

public class CmdUtilTests
{
    [Theory]
    [InlineData(new[] { "mail", "dns", "example.com", "-m" }, true)]
    [InlineData(new[] { "mail", "dns", "--machine" }, true)]
    [InlineData(new[] { "mail", "dns", "example.com" }, false)]
    [InlineData(new[] { "bogus", "-m" }, true)]
    public void IsMachineRequest_Detects_Machine_Tokens(string[] args, bool expected)
    {
        CmdUtil.IsMachineRequest(args).Should().Be(expected);
    }

    [Theory]
    [InlineData(new[] { "-v" }, true)]
    [InlineData(new[] { "--version" }, true)]
    [InlineData(new[] { "-v", "--version" }, true)]
    [InlineData(new string[0], false)]
    [InlineData(new[] { "git", "-v" }, false)]
    [InlineData(new[] { "-v", "git" }, false)]
    [InlineData(new[] { "--help" }, false)]
    public void IsRootVersionRequest_MatchesRootOnlyInvocations(string[] args, bool expected)
    {
        CmdUtil.IsRootVersionRequest(args).Should().Be(expected);
    }
}
