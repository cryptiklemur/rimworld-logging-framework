namespace CryptikLemur.RimLogging.Bundle;

/// <summary>
/// Uploads a <see cref="BundlePayload"/> to the bundle proxy endpoint over HTTP and interprets the response.
/// The proxy is expected to return JSON containing a <c>url</c> field pointing at the created gist.
/// </summary>
public sealed class ProxyClient
{
    private readonly System.Net.Http.HttpClient _http;
    private readonly string _url;

    /// <summary>
    /// Creates a client targeting the given proxy URL. If no <see cref="System.Net.Http.HttpClient"/> is
    /// supplied, a default one with a 30-second timeout is used.
    /// </summary>
    /// <param name="url">The proxy endpoint URL to POST bundles to.</param>
    /// <param name="http">An optional HTTP client to reuse; a default client is created when <c>null</c>.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="url"/> is <c>null</c>.</exception>
    public ProxyClient(string url, System.Net.Http.HttpClient? http = null)
    {
        _url = url ?? throw new System.ArgumentNullException(nameof(url));
        _http = http ?? new System.Net.Http.HttpClient { Timeout = System.TimeSpan.FromSeconds(30) };
    }

    /// <summary>
    /// Serializes and POSTs the payload as JSON to the proxy. On a success status code, the response's <c>url</c>
    /// field is returned as the gist URL. Non-success status codes, malformed JSON, a missing <c>url</c> field,
    /// and transport exceptions are all surfaced as a failed <see cref="ProxyResult"/> rather than thrown.
    /// </summary>
    /// <param name="payload">The bundle to upload.</param>
    /// <returns>A <see cref="ProxyResult"/> describing success with the gist URL, or failure with an error message.</returns>
    public async System.Threading.Tasks.Task<ProxyResult> UploadAsync(BundlePayload payload)
    {
        try
        {
            string json = BundleSerializer.Serialize(payload);
            using System.Net.Http.StringContent content = new System.Net.Http.StringContent(
                json, System.Text.Encoding.UTF8, "application/json");
            using System.Net.Http.HttpResponseMessage resp = await _http.PostAsync(_url, content).ConfigureAwait(false);
            string body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                return new ProxyResult { Success = false, ErrorMessage = $"{(int)resp.StatusCode} {resp.ReasonPhrase}: {body}" };
            }

            try
            {
                using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(body);
                string gistUrl = doc.RootElement.GetProperty("url").GetString() ?? "";
                return new ProxyResult { Success = true, GistUrl = gistUrl };
            }
            catch (System.Text.Json.JsonException jex)
            {
                return new ProxyResult { Success = false, ErrorMessage = $"Malformed response JSON: {jex.Message}" };
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                return new ProxyResult { Success = false, ErrorMessage = "Response JSON missing 'url' field" };
            }
        }
        catch (System.Exception ex)
        {
            return new ProxyResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
