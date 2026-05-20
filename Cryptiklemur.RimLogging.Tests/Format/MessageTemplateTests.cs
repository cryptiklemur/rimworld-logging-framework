// Cryptiklemur.RimLogging.Tests/Format/MessageTemplateTests.cs
using Cryptiklemur.RimLogging.Format;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Format;

public class MessageTemplateTests
{
    [Fact]
    public void Construct_StoresRawTemplate()
    {
        MessageTemplate t = new MessageTemplate(
            raw: "player {Name} died at {Hp}hp",
            holes: new[] { "Name", "Hp" },
            segments: new[] { "player ", " died at ", "hp" });

        Assert.Equal("player {Name} died at {Hp}hp", t.Raw);
        Assert.Equal(new[] { "Name", "Hp" }, t.Holes);
        Assert.Equal(new[] { "player ", " died at ", "hp" }, t.Segments);
    }

    [Fact]
    public void Construct_AllowsEmptyTemplate()
    {
        MessageTemplate t = new MessageTemplate("", new string[0], new[] { "" });
        Assert.Equal("", t.Raw);
        Assert.Empty(t.Holes);
    }
}
