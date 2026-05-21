using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cryptiklemur.RimLogging.Bundle;

public static class BundleSerializer
{
    private static readonly JsonSerializerOptions _opts = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Serialize(BundlePayload p) => JsonSerializer.Serialize(p, _opts);

    public static BundlePayload Deserialize(string json)
        => JsonSerializer.Deserialize<BundlePayload>(json, _opts) ?? new BundlePayload();
}
