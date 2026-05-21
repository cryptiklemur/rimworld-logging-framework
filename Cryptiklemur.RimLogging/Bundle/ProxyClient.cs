namespace Cryptiklemur.RimLogging.Bundle;

public sealed class ProxyClient
{
    private readonly System.Net.Http.HttpClient _http;
    private readonly string _url;

    public ProxyClient(string url, System.Net.Http.HttpClient? http = null)
    {
        _url = url ?? throw new System.ArgumentNullException(nameof(url));
        _http = http ?? new System.Net.Http.HttpClient { Timeout = System.TimeSpan.FromSeconds(30) };
    }

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
