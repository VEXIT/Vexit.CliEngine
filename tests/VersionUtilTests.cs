using System.Text.RegularExpressions;
using FluentAssertions;
using Vexit.CliEngine.Utils;
using Xunit;

namespace Vexit.CliEngine.Tests;

public class VersionUtilTests
{
    [Fact]
    public void GetVersion_ShouldReturnVersionLikeStringOrUnknown()
    {
        var result = VersionUtil.GetVersion();

        result.Should().NotBeNullOrEmpty();
        (result == "unknown" || Regex.IsMatch(result, @"^\d+\.\d+\.\d+$"))
            .Should().BeTrue($"GetVersion() should return a version pattern or 'unknown', but was '{result}'");
    }
}
