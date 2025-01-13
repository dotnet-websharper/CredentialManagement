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
                //"publicKey", T<PublicKeyCredentialCreationOptions>
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
                //"publicKey", T<PublicKeyCredentialRequestOptions>
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

    let CredentialsContainer = 
        Class "CredentialsContainer"
        |+> Instance [
            "create" => !?CreateOptions?options ^-> T<Promise<_>>.[
                    FederatedCredential
                    + PasswordCredential
                    //+ T<PublicKeyCredential>
                ]
            "get" => !?CredentialRequestOptions?options ^-> T<Promise<_>>.[
                    PasswordCredential
                    + IdentityCredential
                    + FederatedCredential
                    + OTPCredential
                    //+ T<PublicKeyCredential>
                ]
            "preventSilentAccess" => T<unit> ^-> T<Promise<unit>>
            "store" => Credential?credentials ^-> T<Promise<unit>>
        ]

    let Navigator =
        Class "Navigator"
        |+> Instance [
            "credentials" =? CredentialsContainer
        ]

    let CredentialManagement = 
        Class "CredentialManagement"
        |+> Static [
            "navigator" =? Navigator
        ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.CredentialManagement" [
                Enum.CredentialTypes
                Enum.Context
                Enum.Mediation

                CredentialManagement
                Navigator
                CredentialsContainer
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
