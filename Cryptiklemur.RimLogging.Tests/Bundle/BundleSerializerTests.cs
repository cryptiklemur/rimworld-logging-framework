using System.Collections.Generic;
using Cryptiklemur.RimLogging.Bundle;
using Xunit;

namespace Cryptiklemur.RimLogging.Tests.Bundle;

public class BundleSerializerTests
{
    [Fact]
    public void Serialize_Roundtrip_PreservesFields()
    {
        BundlePayload p = new BundlePayload
        {
            RimWorldVersion = "1.6.4444",
            FrameworkVersion = "v1.0.0-beta",
        };
        p.Mods.Add(new BundlePayload.ModInfo { Name = "Core", PackageId = "Ludeon.RimWorld", Active = true });
        p.Entries.Add(new BundlePayload.EntryDto
        {
            Ts = "2026-05-20T00:00:00Z",
            Level = "Info",
            Channel = "default",
            Source = "Foo.cs:1",
            Msg = "hello",
            Ctx = new Dictionary<string, object?> { ["user"] = "alice" },
        });

        string json = BundleSerializer.Serialize(p);
        BundlePayload back = BundleSerializer.Deserialize(json);

        Assert.Equal(p.RimWorldVersion, back.RimWorldVersion);
        Assert.Equal(p.FrameworkVersion, back.FrameworkVersion);
        Assert.Single(back.Mods);
        Assert.Equal("Core", back.Mods[0].Name);
        Assert.Single(back.Entries);
        Assert.Equal("hello", back.Entries[0].Msg);
        Assert.NotNull(back.Entries[0].Ctx);
        Assert.Equal("alice", back.Entries[0].Ctx!["user"]?.ToString());
    }

    [Fact]
    public void Serialize_NullCtx_OmittedFromOutput()
    {
        BundlePayload p = new BundlePayload();
        p.Entries.Add(new BundlePayload.EntryDto { Ts = "t", Level = "Info", Channel = "c", Source = "", Msg = "m", Ctx = null });
        string json = BundleSerializer.Serialize(p);
        Assert.DoesNotContain("\"ctx\"", json);
    }

    [Fact]
    public void Serialize_NullStack_OmittedFromOutput()
    {
        BundlePayload p = new BundlePayload();
        p.Entries.Add(new BundlePayload.EntryDto { Ts = "t", Level = "Info", Channel = "c", Source = "", Msg = "m", Stack = null });
        string json = BundleSerializer.Serialize(p);
        Assert.DoesNotContain("\"stack\"", json);
    }

    [Fact]
    public void Serialize_RichTextNotAutoStripped()
    {
        BundlePayload p = new BundlePayload();
        p.Entries.Add(new BundlePayload.EntryDto { Ts = "t", Level = "Info", Channel = "c", Source = "", Msg = "<color=red>x</color>" });
        string json = BundleSerializer.Serialize(p);
        Assert.Contains("<color=red>x</color>", json);
    }
}
