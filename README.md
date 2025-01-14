# WebSharper Credential Management API Binding

This repository offers an F# [WebSharper](https://websharper.com/) binding for the [Credential Management API](https://developer.mozilla.org/en-US/docs/Web/API/Credential_Management_API). By leveraging this API, developers can streamline user authentication and securely manage credentials in web applications, all while integrating seamlessly into WebSharper projects.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:
   - Contains the F# WebSharper binding for the Credential Management API.

2. **Sample Project**:
   - Demonstrates how to use the Credential Management API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/CredentialManagement/).

## Features

- WebSharper bindings for the Credential Management API.
- Example usage through the Sample project.
- Hosted demo to explore API functionality.

## Installation and Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.
- Node.js and npm (for building web assets).
- WebSharper tools.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/CredentialManagement.git
   cd CredentialManagement
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.CredentialManagement/WebSharper.CredentialManagement.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.CredentialManagement.Sample
   dotnet build
   dotnet run
   ```

4. Open the hosted demo to see the Sample project in action:
   [https://dotnet-websharper.github.io/CredentialManagement/](https://dotnet-websharper.github.io/CredentialManagement/)

## Why Use the Credential Management API

The Credential Management API is a browser-native solution that simplifies credential management in modern web applications. Key benefits include:

1. **Unified Authentication**: Handles passwords, federated logins, and public key credentials in a single API.
2. **Enhanced Security**: Minimizes phishing risks by reducing manual credential entry and supports passwordless sign-in with WebAuthn.
3. **User Convenience**: Streamlines the login process with automatic sign-in and retrieval of stored credentials.
4. **Standardized Approach**: Provides a consistent, browser-native way to manage credentials across different platforms.

Integrating the Credential Management API with WebSharper allows developers to create secure, user-friendly authentication flows in F# web applications.

## How to Use the Credential Management API

### Overview

The Credential Management API enables secure and user-friendly authentication by providing a standardized way to handle credentials. By integrating this API, you can enhance user experience and security in your WebSharper applications.

### Example Usage

The Credential Management API allows seamless handling of user credentials in web applications. Below is an example of how to use it with WebSharper:

```fsharp
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.CredentialManagement

// Define the connection to the HTML template
// `IndexTemplate` binds to `wwwroot/index.html` and enables dynamic updates to the UI

type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

[<JavaScript>]
module Client =
    let username = Var.Create ""
    let password = Var.Create ""

    // Retrieves the browser's credentials container, providing access to the Credential Management API
    let credentialsContainer = As<Navigator>(JS.Window.Navigator).Credentials

    // This function retrieves credentials from the browser's Credential Management API
    let retrieveCredentials () =
        promise {
            try
                // Sets up options for retrieving credentials, specifying that passwords are included and mediation is optional
                let credentialRequestOptions = CredentialRequestOptions(
                    Password = true,
                    Mediation = Mediation.Optional
                )

                let! credential = As<Promise<PasswordCredential>> <| credentialsContainer.Get(credentialRequestOptions) 
                if not (isNull credential) then
                    Console.Log("Retrieved credentials:", credential)
                
                    // Check if Id and Password are valid before assigning
                    // This ensures only valid and non-null data is used, avoiding potential runtime errors
                    if not (isNull credential.Id) && not (isNull credential.Password) then
                        Console.Log($"Credential id: {credential.Id}")
                        Console.Log($"Credential Password: {credential.Password}")

                        username.Value <- credential.Id
                        password.Value <- credential.Password
                    else
                        Console.Log("Credential Id is undefined.")
                        Console.Log("Credential Password is undefined.")
                else
                    Console.Log("No stored credentials found.")
            with ex ->
                Console.Error($"Failed to retrieve credentials: {ex.Message}")
        }

    [<SPAEntryPoint>]
    let Main () =

        // Auto-fill credentials on page load
        async {
            do! retrieveCredentials() |> Promise.AsAsync
        }
        |> Async.StartImmediate

        // Binds the username and password variables to their corresponding fields in the HTML template
        IndexTemplate.Main()
            .Username(username.V)
            .Password(password.V)
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to retrieve credentials using the `navigator.credentials.get()` method, handle different credential types, and log the results or errors to the console.

For a complete implementation, refer to the [Sample Project](https://dotnet-websharper.github.io/CredentialManagement/).
