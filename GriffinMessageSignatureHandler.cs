namespace GriffinMessageSignature;

public class GriffinMessageSignatureHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Content ??= new StringContent(string.Empty, System.Text.Encoding.UTF8, "application/json");
        request.Headers.TryAddWithoutValidation("Date", GetRfc1123Date());

        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }

    private static string GetRfc1123Date() => DateTimeOffset.Now.ToString("r");
}
