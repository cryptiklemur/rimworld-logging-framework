using Cryptiklemur.RimLogging.Filters;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Filters;

public class ChannelRegistryTests
{
    [Theory]
    [InlineData("combat")]
    [InlineData("default")]
    [InlineData("")]
    [InlineData("some.nested.channel")]
    public void TryResolve_AnyChannel_ReturnsNull(string channel)
    {
        ChannelDef? result = ChannelRegistry.TryResolve(channel);

        Assert.Null(result);
    }
}
