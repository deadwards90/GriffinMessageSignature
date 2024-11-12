using System.Text.Json;
using System.Text.Json.Serialization;

namespace GriffinMessageSignature;

public class GriffinClient
{
    private readonly HttpClient _client;

    public GriffinClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<HttpResponseMessage> SignatureDebugPost()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "v0/security/message-signature/verify");

        var requestContent = new StringContent("{\"hello\": \"world\"}",
            System.Text.Encoding.UTF8, "application/json");
        request.Content = requestContent;

        var response = await _client.SendAsync(request);
        return response;
    }

    public async Task<HttpResponseMessage> SignatureDebugGet()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "v0/security/message-signature/verify");
        var response = await _client.SendAsync(request);
        return response;
    }
}
