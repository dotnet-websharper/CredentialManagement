namespace WebSharper.CredentialManagement.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.CredentialManagement

type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so you can edit index.html and refresh
    // the browser; no need to recompile unless you add or remove holes.
    let username = Var.Create ""
    let password = Var.Create ""
    let status = Var.Create "Ready."

    let credentialsContainer = JS.Window.Navigator.Credentials

    // The Credential Management API is only exposed in secure contexts (HTTPS or
    // localhost) and not in every browser, so report the situation instead of
    // throwing when the sample is opened elsewhere.
    let isSupported () =
        not (isNull credentialsContainer) && JS.Window.IsSecureContext

    // Save credentials using the Credential Management API.
    let saveCredentials (userName: string) (passWord: string) =
        promise {
            try
                Console.Log($"Attempting to save credentials for username: {userName}")

                let passwordCredentialData =
                    PasswordCredentialData(
                        Id = userName,
                        Password = passWord,
                        Name = userName
                    )

                let credential = new PasswordCredential(passwordCredentialData)

                do! credentialsContainer.Store(credential) |> Promise.AsAsync
                status.Value <- "Credentials stored successfully."
            with ex ->
                Console.Error($"Failed to save credentials: {ex.Message}")
                status.Value <- $"Could not store credentials: {ex.Message}"
        }

    let retrieveCredentials () =
        promise {
            try
                let credentialRequestOptions =
                    CredentialRequestOptions(
                        Password = true,
                        Mediation = Mediation.Optional
                    )

                let! credential = As<Promise<PasswordCredential>> <| credentialsContainer.Get(credentialRequestOptions)

                if not (isNull credential) then
                    // Check that Id and Password are valid before assigning, so we
                    // never push null values into the UI.
                    if not (isNull credential.Id) && not (isNull credential.Password) then
                        username.Value <- credential.Id
                        password.Value <- credential.Password
                        status.Value <- "Stored credentials loaded."
                    else
                        status.Value <- "Stored credential is missing its id or password."
                else
                    status.Value <- "No stored credentials found."
            with ex ->
                Console.Error($"Failed to retrieve credentials: {ex.Message}")
                status.Value <- $"Could not retrieve credentials: {ex.Message}"
        }

    [<SPAEntryPoint>]
    let Main () =
        if not (isSupported ()) then
            status.Value <- "The Credential Management API is unavailable. Use a modern browser over HTTPS or localhost."
        else
            // Auto-fill credentials on page load.
            async {
                do! retrieveCredentials () |> Promise.AsAsync
            }
            |> Async.StartImmediate

        IndexTemplate.Main()
            .Username(username)
            .Password(password)
            .Status(status.V)
            .Submit(fun e ->
                e.Event.PreventDefault()

                if not (isSupported ()) then
                    status.Value <- "The Credential Management API is unavailable. Use a modern browser over HTTPS or localhost."
                else
                    promise {
                        let userName = username.Value
                        let passWord = password.Value

                        if System.String.IsNullOrWhiteSpace(userName) || System.String.IsNullOrWhiteSpace(passWord) then
                            status.Value <- "Please fill in both username and password."
                        else
                            do! saveCredentials userName passWord
                    }
                    |> Promise.AsAsync
                    |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
