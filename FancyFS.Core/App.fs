namespace FancyFS.Core

open Microsoft.FSharp.Collections

type IUser =
    interface
    end

type Cookie =
    {
        Name : string
        Value : string
        Domain : string
        Path : string
        Expires : System.DateTime
        Secure : bool
        Shareable : bool
        SubKeys : Map<string, string>
    }

type Request =
    {
        Cookies : Cookie list
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

type IPipelineLocation =
    abstract member Pipeline : Pipeline with get

module App =
    
    let private defaultResponse = { Headers = Map.empty; StatusCode = None; Body = ""; }

    let BaseRequest =
        let func (req, resp) =(req, resp)
        { Element = func; Next = None; } 

    let (==>) pipeline func =
        let p = { Element = func; Next = None; }
        { pipeline with Next = Some p; }

    let FindPipeline () =
        let pipelineInterface = typeof<IPipelineLocation>
        let pipelineLocs = System.AppDomain.CurrentDomain.GetAssemblies()
                           |> Seq.map (fun t -> t.GetTypes())
                           |> Seq.concat
                           |> Seq.filter (fun t -> pipelineInterface.IsAssignableFrom(t))
                           |> Seq.filter (fun t -> not (t.Assembly = pipelineInterface.Assembly))
                           |> Seq.head
        let pipelineLocInst = System.Activator.CreateInstance(pipelineLocs) :?> IPipelineLocation
        pipelineLocInst

    let ExecutePipeline pipeline request =
        let rec ExecuteElement req resp elem =
            let req, resp = elem.Element (req, resp)
            match elem.Next with
            | Some x -> let req, resp = x.Element (req, resp)
                        resp
            | None -> resp
        ExecuteElement request defaultResponse pipeline 