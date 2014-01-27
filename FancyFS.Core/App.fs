namespace FancyFS.Core

open Microsoft.FSharp.Collections

type IUser =
    interface
    end

type Request =
    {
        Headers : Map<string, string>
        QueryString : Map<string, string>
        Path : System.Uri
        User : IUser option
    }

type Response =
    {
        Headers : Map<string, string>
        StatusCode : System.Net.HttpStatusCode option
        Body : string
    }