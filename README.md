# GriffinMessageSignature

## Prerequisites

- .NET 8 SDK
- Ed25519 public and private key .pem files
- Griffin API Key
- Griffin message signature signing key

## Usage

1. Fill in the `appsettings.json` file with the relevant settings
2. Run the application

By default, the application will log out the requests and responses that are made by the `HttpClient`.

## Useful information

- `Program.cs` contains the bulk of the setup of required for NSign.
  - Uses Sha512 for the content hashing
  - Uses BouncyCastle for the Ed25519 implementation
  - Implementation is based off the samples NSign have along with the required components in the Griffin documentation
- `GriffinClient.cs` contains the client for communicating with the Griffin API
- `GriffinMessageSignatureHandler.cs` handles adding the `Date` header, along with an empty body for cases such as `GET` where it is required.
