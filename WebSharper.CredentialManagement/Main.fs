namespace WebSharper.CredentialManagement

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator
//open WebSharper.WebAuthentication

module Definition =

    module Enum = 
        let CredentialTypes = 
            Pattern.EnumStrings "CredentialTypes" [
                "password"
                "federated"
                "public-key"
                "identity"
                "otp"
            ]

        let Mediation = 
            Pattern.EnumStrings "Mediation" [
                "conditional"
                "optional"
                "required"
                "silent"
            ]

        let Context = 
            Pattern.EnumStrings "Context" [
                "continue"
                "signin"
                "signup"
                "use"
            ]

    let Credential = 
        Class "Credential"
        |+> Static [
            "isConditionalMediationAvailable" => T<unit> ^-> T<Promise<bool>>
        ]
        |+> Instance [
            "id" =? T<string>
            "type" =? Enum.CredentialTypes.Type
        ]

    let FederatedCredentialInit =
        Pattern.Config "FederatedCredentialInit" {
            Required = [
                "id", T<string>
                "origin", T<string>
                "provider", T<string>
            ]
            Optional = [
                "iconURL", T<string>
                "name", T<string>
                "protocol", T<string>
            ]
        }

    let PasswordCredentialInit =
        Pattern.Config "PasswordCredentialInit" {
            Required = [
                "id", T<string>
                "password", T<string>
                "origin", T<string>
            ]
            Optional = [
                "iconURL", T<string>
                "name", T<string>
            ]
        }

    let CreateOptions =
        Pattern.Config "CreateOptions" {
            Required = []
            Optional = [
                "signal", T<Dom.AbortSignal>
                "federated", FederatedCredentialInit.Type
                "password", PasswordCredentialInit.Type
                // The full PublicKeyCredentialCreationOptions graph is not modeled yet;
                // accept a JS object so WebAuthn creation options can still be passed.
                "publicKey", T<obj>
            ]
        }

    let FederatedCredentialInitOptions = 
        Pattern.Config "FederatedCredentialInitOptions" {
            Required = [
                "provider", T<string>
            ]
            Optional = []
        }

    let FederatedCredential = 
        Class "FederatedCredential" 
        |=> Inherits Credential
        |+> Static [
            Constructor FederatedCredentialInitOptions?init
            // FederatedCredential() also accepts the full FederatedCredentialInit dictionary.
            Constructor FederatedCredentialInit?init
        ]
        |+> Instance [
            "protocol" =? T<string>
            "provider" =? T<string>
        ]

    let PasswordCredentialData =
        Pattern.Config "PasswordCredentialData" {
            Required = []
            Optional = [
                "id", T<string>         
                "password", T<string>   
                "origin", T<string> 
                "iconURL", T<string>    
                "name", T<string>       
            ]
        }

    let PasswordCredential = 
        Class "PasswordCredential"
        |=> Inherits Credential
        |+> Static [
            Constructor PasswordCredentialData?data
            Constructor T<HTMLFormElement>?form
        ]
        |+> Instance [
            "password" =? T<string>   
            "name" =? T<string>       
            "iconURL" =? T<string>    
        ]

    let Providers = 
        Pattern.Config "Providers" {
            Required = [
                "configURL", T<string>
                "clientId", T<string>
            ]
            Optional = [
                "loginHint", T<string>
                "nonce", T<string>
            ]
        }

    let IdentityCredentialRequestOptions = 
        Pattern.Config "IdentityCredentialRequestOptions" {
            Required = []
            Optional = [
                "context", Enum.Context.Type
                "providers", Providers.Type
            ]
        }

    let FederatedCredentialRequestOptions = 
        Pattern.Config "FederatedCredentialRequestOptions" {
            Required = [
                "protocols", !| T<string>
                "providers", !| T<string>
            ]
            Optional = []
        }

    let CredentialRequestOptions = 
        Pattern.Config "CredentialRequestOptions" {
            Required = []
            Optional = [
                "mediation", Enum.Mediation.Type
                "signal", T<Dom.AbortSignal> 
                "password", T<bool> 
                "identity", IdentityCredentialRequestOptions.Type
                "federated", FederatedCredentialRequestOptions.Type
                "otp", !| T<string> 
                // The full PublicKeyCredentialRequestOptions graph is not modeled yet;
                // accept a JS object so WebAuthn request options can still be passed.
                "publicKey", T<obj>
            ]
        }

    let IdentityCredential =
        Class "IdentityCredential"
        |=> Inherits Credential
        |+> Instance [
            "isAutoSelected" =? T<bool>
            "token" =? T<string>
        ]

    let OTPCredential =
        Class "OTPCredential"
        |=> Inherits Credential
        |+> Instance [
            "code" =? T<string>
        ]

    let PublicKeyCredential =
        Class "PublicKeyCredential"
        |=> Inherits Credential
        |+> Static [
            "isConditionalMediationAvailable" => T<unit> ^-> T<Promise<bool>>
            "isUserVerifyingPlatformAuthenticatorAvailable" => T<unit> ^-> T<Promise<bool>>
        ]
        |+> Instance [
            "authenticatorAttachment" =? T<string>
            "rawId" =? T<JavaScript.ArrayBuffer>
            // The AuthenticatorResponse object graph is part of the WebAuthn API and is
            // not modeled here yet, so it is exposed as an opaque object.
            "response" =? T<obj>
            "getClientExtensionResults" => T<unit> ^-> T<obj>
            "toJSON" => T<unit> ^-> T<obj>
        ]

    let CredentialsContainer = 
        Class "CredentialsContainer"
        |+> Instance [
            "create" => !?CreateOptions?options ^-> T<Promise<_>>.[
                    FederatedCredential
                    + PasswordCredential
                    + PublicKeyCredential
                ]
            "get" => !?CredentialRequestOptions?options ^-> T<Promise<_>>.[
                    PasswordCredential
                    + IdentityCredential
                    + FederatedCredential
                    + OTPCredential
                    + PublicKeyCredential
                ]
            "preventSilentAccess" => T<unit> ^-> T<Promise<unit>>
            // requireUserMediation() is the earlier spec name for preventSilentAccess();
            // kept for source compatibility and marked obsolete.
            "requireUserMediation" => T<unit> ^-> T<Promise<unit>>
            |> ObsoleteWithMessage "requireUserMediation() is the older name for preventSilentAccess(); use preventSilentAccess() instead."
            "store" => Credential?credentials ^-> T<Promise<unit>>
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.CredentialManagement" [
                Enum.CredentialTypes
                Enum.Context
                Enum.Mediation

                CredentialsContainer
                PublicKeyCredential
                OTPCredential
                IdentityCredential
                CredentialRequestOptions
                FederatedCredentialRequestOptions
                IdentityCredentialRequestOptions
                Providers
                PasswordCredential
                PasswordCredentialData
                FederatedCredential
                FederatedCredentialInitOptions
                CreateOptions
                PasswordCredentialInit
                FederatedCredentialInit
                Credential
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
