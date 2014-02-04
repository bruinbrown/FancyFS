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

type Pipeline = Async<Request * Response> -> Async<Request * Response>

type IPipelineLocation =
    abstract member Pipeline : Pipeline with get

module PipelineModule =
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

    (* The reason for a custom operator for now is that it leaves the potential for adding in steps which may
       happen between pipeline functions. E.g. we may choose to return an indication of whether the 
       pipeline should terminate *)
    let (==>) (f:Async<Request * Response> -> Async<Request * Response>) g (x:Async<Request * Response>) =
        let res = f x
        g res

    let ExecutePipelineAsync pipeline request initialResponse =
        let asyncInput =
            async {
                return (request, initialResponse)
            }
        pipeline asyncInput

module RequestResponseModule =
    let DefaultResponse = { Headers = Map.empty; StatusCode = None; Body = ""; }

    let EmptyRequest = { Headers = Map.empty; Cookies = []; QueryString = Map.empty; Path = System.Uri("http://www.google.com"); User = None }