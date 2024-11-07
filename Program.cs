using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using GriffinMessageSignature;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Diagnostics;
using Microsoft.Extensions.Logging;
using NSign;
using NSign.BouncyCastle.Providers;
using NSign.Client;
using NSign.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using ISigner = NSign.ISigner;

var configuration = new ConfigurationBuilder()
     .AddJsonFile("appsettings.json", optional: false)
     .Build();

var privateKeyFilePath = configuration["PrivateKeyFilePath"] ?? throw new InvalidOperationException("PrivateKeyFilePath is required");
var publicKeyFilePath = configuration["PublicKeyFilePath"] ?? throw new InvalidOperationException("PublicKeyFilePath is required");
var apiKey = configuration["ApiKey"] ?? throw new InvalidOperationException("ApiKey is required");
var signingKeyId = configuration["SigningKeyId"] ?? throw new InvalidOperationException("SigningKeyId is required");

var services = new ServiceCollection()
     .AddLogging(o => o.SetMinimumLevel(LogLevel.Trace)
          .AddJsonConsole(json =>
               json.JsonWriterOptions = new JsonWriterOptions
               {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Indented = true
               })
     )
     // Http Client setup
     .AddHttpClient()
     .AddRedaction()
     .AddExtendedHttpClientLogging(o =>
     {
          o.LogBody = true;
          o.LogRequestStart = true;
          o.ResponseBodyContentTypes.Add("application/json");
          o.RequestPathParameterRedactionMode = HttpRouteParameterRedactionMode.None;
     })

     // NSign setup
     .Configure<AddContentDigestOptions>(o =>
     {
          o.WithHash(AddContentDigestOptions.Hash.Sha512);
     })
     .ConfigureMessageSigningOptions(o =>
     {
          o.SignatureName = "wk-grf";
          var signingOptions = o
               .WithMandatoryComponent(SignatureComponent.Authority)
               .WithMandatoryComponent(SignatureComponent.ContentDigest)
               .WithMandatoryComponent(SignatureComponent.ContentLength)
               .WithMandatoryComponent(new HttpHeaderComponent(Constants.Headers.ContentType))
               .WithMandatoryComponent(new HttpHeaderComponent("Date"))
               .WithMandatoryComponent(SignatureComponent.Method)
               .WithMandatoryComponent(SignatureComponent.Path)
               .WithMandatoryComponent(SignatureComponent.RequestTargetUri);

          signingOptions.SetParameters = signatureParams =>
          {
               signatureParams.Algorithm = "ed25519";
               signatureParams.KeyId = signingKeyId;
               signatureParams.WithCreated(DateTimeOffset.Now);
               signatureParams.WithExpires(DateTimeOffset.Now.AddMinutes(4));
               signatureParams.WithNonce(Guid.NewGuid().ToString());
          };
     })
     .Services
     .AddSingleton<ISigner>(s =>
          new EdDsaEdwards25519SignatureProvider(
               GetKeyParameterFromFile<Ed25519PrivateKeyParameters>(privateKeyFilePath),
               GetKeyParameterFromFile<Ed25519PublicKeyParameters>(publicKeyFilePath),
               signingKeyId
          )
     )

     // Griffin API Client setup with NSign
     .AddTransient<GriffinMessageSignatureHandler>()
     .AddHttpClient<GriffinClient>(client =>
     {
          client.BaseAddress = new Uri("https://api.griffin.com/");
          client.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("GriffinAPIKey", apiKey);
     })
     .AddHttpMessageHandler<GriffinMessageSignatureHandler>()
     .AddContentDigestAndSigningHandlers()
     .Services
     .BuildServiceProvider();


var griffinClient = services.GetRequiredService<GriffinClient>();

await griffinClient.SignatureDebugPost();
await griffinClient.SignatureDebugGet();

Console.WriteLine();

Console.WriteLine("Done, press any key to exit");
Console.ReadKey();

return;


static T GetKeyParameterFromFile<T>(string path) where T : AsymmetricKeyParameter
{
     using var reader = File.OpenText(path);
     var pemReader = new PemReader(reader);
     var key = pemReader.ReadObject();
     return (T)key;
}
