namespace FancyFS.Core

open Microsoft.FSharp.Collections

type IUser =
    interface
    end

type Request =
    {
        Cookies : Map<string, string>
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
        Element : ((Request * Response) -> (Request * Response))
        Next : Pipeline option
    }

module App =
    
    let private defaultResponse = { Headers = Map.empty; StatusCode = None; Body = ""; }

    let BaseRequest =
        let func (req, resp) =(req, resp)
        { Element = func; Next = None; } 

    let (==>) pipeline func =
        let p = { Element = func; Next = None; }
        { pipeline with Next = Some p; }

    let ExecutePipeline pipeline request =
        let rec ExecuteElement req resp elem =
            let req, resp = elem.Element (req, resp)
            match resp.StatusCode with
            | Some _ -> resp
            | None -> match elem.Next with
                      | Some x -> let req, resp = x.Element (req, resp)
                                  resp
                      | None -> resp
        ExecuteElement request defaultResponse pipeline 