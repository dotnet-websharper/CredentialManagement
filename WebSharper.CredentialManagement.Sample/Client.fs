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
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    
    let username = Var.Create ""
    let password = Var.Create ""

    let credentialsContainer = As<Navigator>(JS.Window.Navigator).Credentials

    // Save credentials using the Credential Management API
    let saveCredentials (username: string) (password: string) =
        promise {
            Console.Log($"Attempting to save credentials for username: {username}")
        
            let passwordCredentialData = PasswordCredentialData(
                Id = username,
                Password = password,
                Name = username
            )
        
            let credential = new PasswordCredential(passwordCredentialData)
        
            try
                Console.Log("Before storing credential:", credential)
                do! credentialsContainer.Store(credential) |> Promise.AsAsync
                Console.Log("Credentials stored successfully!")
            with ex ->
                Console.Error($"Failed to save credentials: {ex.Message}")
        } 

    let retrieveCredentials () =
        promise {
            try
                let credentialRequestOptions = CredentialRequestOptions(
                    Password = true,
                    Mediation = Mediation.Optional
                )

                let! credential = As<Promise<PasswordCredential>> <| credentialsContainer.Get(credentialRequestOptions) 
                if not (isNull credential) then
                    Console.Log("Retrieved credentials:", credential)
                
                    // Check if Id and Password are valid before assigning
                    if not (isNull credential.Id) then
                        Console.Log($"Credential id: {credential.Id}")
                        username.Value <- credential.Id
                    else
                        Console.Log("Credential Id is undefined.")

                    if not (isNull credential.Password) then
                        Console.Log($"Credential Password: {credential.Password}")
                        password.Value <- credential.Password
                    else
                        Console.Log("Credential Password is undefined.")
                else
                    Console.Log("No stored credentials found.")
            with ex ->
                Console.Error($"Failed to retrieve credentials: {ex.Message}")
        }

    let handleLoginFormSubmit (event: Dom.Event) =
        promise {
            event.PreventDefault()
            let usernameValue = username.Value
            let passwordValue = password.Value
            
            if not (System.String.IsNullOrWhiteSpace(usernameValue) || System.String.IsNullOrWhiteSpace(passwordValue)) then
                do! saveCredentials usernameValue passwordValue
                JS.Alert("Logged in successfully!")
            else
                JS.Alert("Please fill in both username and password.")
        }

    [<SPAEntryPoint>]
    let Main () =

        // Auto-fill credentials on page load
        async {
            do! retrieveCredentials() |> Promise.AsAsync
        }
        |> Async.StartImmediate

        IndexTemplate.Main()
            .Username(username.V)
            .Password(password.V)
            .formInit(fun () -> 
                async {
                    let loginForm = As<HTMLElement> <| JS.Document.GetElementById("loginForm")                    

                    // Attach the login form submission handler
                    loginForm.AddEventListener("submit", fun (e: Dom.Event) -> 
                        async {
                            do! handleLoginFormSubmit e |> Promise.AsAsync
                        }
                        |> Async.StartImmediate
                    )
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
