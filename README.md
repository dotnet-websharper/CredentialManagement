# WebSharper Credential Management API Binding

[![NuGet](https://img.shields.io/nuget/v/WebSharper.CredentialManagement.svg)](https://www.nuget.org/packages/WebSharper.CredentialManagement)

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Credential Management API](https://developer.mozilla.org/en-US/docs/Web/API/Credential_Management_API), exposing password, federated, one-time-password and public-key credentials to WebSharper applications.

> **Browser support and secure context:** The Credential Management API is only exposed in [secure contexts](https://developer.mozilla.org/en-US/docs/Web/Security/Defenses/Secure_Contexts) (HTTPS or localhost), and browser support varies. Check [MDN](https://developer.mozilla.org/en-US/docs/Web/API/Credential_Management_API) for the current compatibility matrix. The sample reports when the API is unavailable instead of failing silently.

## Repository structure

The repository contains three projects:

| Project | What it is |
| --- | --- |
| `WebSharper.CredentialManagement` | The WIG binding: `CredentialsContainer`, `Credential`, `PasswordCredential`, `FederatedCredential`, `PublicKeyCredential`, `IdentityCredential`, `OTPCredential` and their option types. |
| `WebSharper.CredentialManagement.Extensions` | Hand-written `[<Inline>]` code — currently the `Navigator.Credentials` extension property. |
| `WebSharper.CredentialManagement.Sample` | An ASP.NET Core SPA that exercises the binding end to end. See [Running the test case](#running-the-test-case). |

## Live demos

- Binding sample (GitHub Pages): <https://dotnet-websharper.github.io/CredentialManagement/>
- Source: <https://github.com/dotnet-websharper/CredentialManagement>
- WebSharper bindings catalog: <https://websharper.com/bindings>

## Installation

```bash
dotnet add package WebSharper.CredentialManagement
```

The package targets `netstandard2.0` and works with any WebSharper 10 project.

## Usage

All types live in the `WebSharper.CredentialManagement` namespace. `JS.Window.Navigator.Credentials` gives you the browser's `CredentialsContainer`; `get`, `create`, `store` and `preventSilentAccess` return `Promise`s.

```fsharp
open WebSharper
open WebSharper.JavaScript
open WebSharper.CredentialManagement

[<JavaScript>]
let saveAndLoad () =
    promise {
        let container = JS.Window.Navigator.Credentials

        // Store a password credential.
        let data = PasswordCredentialData(Id = "alice", Password = "s3cret", Name = "Alice")
        let credential = new PasswordCredential(data)
        do! container.Store(credential) |> Promise.AsAsync

        // Ask the browser for a stored password credential.
        let options = CredentialRequestOptions(Password = true, Mediation = Mediation.Optional)
        let! found = As<Promise<PasswordCredential>> <| container.Get(options)

        if not (isNull found) then
            Console.Log(found.Id)
    }
```

The binding also models `FederatedCredential` and `PublicKeyCredential` (WebAuthn). The full
`PublicKeyCredentialCreationOptions` / `PublicKeyCredentialRequestOptions` object graphs are not
typed yet; pass them as JavaScript objects through the `PublicKey` field of `CreateOptions` /
`CredentialRequestOptions`.

> `requireUserMediation()` is the earlier spec name for `preventSilentAccess()`. It is kept for
> source compatibility and marked obsolete; new code should call `preventSilentAccess()`.

## Running the test case

The sample doubles as the repository's test case. It covers:

- feature-detecting `navigator.credentials` and `window.isSecureContext`, and reporting when the
  API is unavailable;
- loading a stored password credential on page load and binding it to the form's Vars;
- storing a password credential through `credentials.store()` when the form is submitted;
- rendering the outcome through a reactive `Var` instead of `alert()`.

Build and run it:

```bash
cd WebSharper.CredentialManagement.Sample
dotnet run
```

Then open the URL printed in the console. On `localhost` the page is a secure context; fill in a
username and password and press **Login** to exercise the store/get round-trip. In browsers or
origins where the API is unavailable, the status line explains why.

## Building from source

Prerequisites: [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later.

```bash
git clone https://github.com/dotnet-websharper/CredentialManagement.git
cd CredentialManagement
dotnet build WebSharper.CredentialManagement/WebSharper.CredentialManagement.fsproj
dotnet build WebSharper.CredentialManagement.Sample/WebSharper.CredentialManagement.Sample.fsproj
```

## Deployment

`.github/workflows/ghpages.yml` builds the sample in Release on every push to `main` and publishes
`WebSharper.CredentialManagement.Sample/wwwroot` to GitHub Pages.

## License

Apache 2.0 — see the [WebSharper core repository](https://github.com/dotnet-websharper/core/blob/master/LICENSE.md).
