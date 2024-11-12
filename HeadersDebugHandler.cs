using Microsoft.Extensions.Logging;

namespace GriffinMessageSignature;

public class HeadersDebugHandler : DelegatingHandler
{
    private readonly ILogger<HeadersDebugHandler> _logger;

    public HeadersDebugHandler(ILogger<HeadersDebugHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LogHeaders(request);

        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }

    private static void LogHeaders(HttpRequestMessage request)
    {
        foreach (var header in request.Headers.Concat(request.Content?.Headers
                                                      ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                     .OrderBy(h => h.Key))
        {
            Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }
    }
}
