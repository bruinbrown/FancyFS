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

type Pipeline =
    {
        Element : (Request -> Response -> Response)
        Next : Pipeline option
    }

module App =
    
    let private defaultResponse = { Headers = Map.empty; StatusCode = None; Body = ""; }

    let (==>) pipeline func =
        let p = { Element = func; Next = None; }
        { pipeline with Next = Some p; }

    let ExecutePipeline pipeline request =
        let rec ExecuteElement req resp elem =
            let r = elem.Element req resp
            match r.StatusCode with
            | Some _ -> r
            | None -> match elem.Next with
                      | Some x -> x.Element req r
                      | None -> r
        ExecuteElement request defaultResponse pipeline 