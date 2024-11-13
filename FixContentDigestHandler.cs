namespace GriffinMessageSignature;

/// <summary>
/// Left here just for reference, this was what was previously fixing the issue with NSec
/// around the content digest header
/// going from
/// SHA-512=HashHere==
/// to
/// sha-512:HashHere==:
/// </summary>
public class FixContentDigestHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IEnumerable<string>? digestHeader = [];

        var noContentDigest = !(request.Content?.Headers.TryGetValues("Content-Digest", out digestHeader) ?? false);
        if (noContentDigest)
        {
            return base.SendAsync(request, cancellationToken);
        }

        var digestValue = digestHeader!.First();
        var digestSplit = digestValue.Split("=", 2);
        var hashName = digestSplit[0].ToLowerInvariant();
        var hash = $"{hashName}=:{digestSplit[1]}:";

        request.Content!.Headers.Remove("Content-Digest");
        request.Content.Headers.Add("Content-Digest", hash);

        return base.SendAsync(request, cancellationToken);
    }
}
