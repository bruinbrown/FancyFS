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

    (* Being able to terminate a response early is important in the case of handling the likes of static content
       Should we have found a file match for some static content then we don't want to returna  404 if the same
       route isn't found in the router *)
    let (==>) (f:Async<Request * Response> -> Async<Request * Response>) g (x:Async<Request * Response>) =
        let next = async {
            let fromFirst = f x
            let! req, resp = fromFirst
            let! response = match resp.StatusCode with
                            | Some x -> fromFirst
                            | None -> g fromFirst
            return response
        }
        next

    let ExecutePipelineAsync pipeline request initialResponse =
        let asyncInput =
            async {
                return (request, initialResponse)
            }
        pipeline asyncInput

module RequestResponseModule =
    let DefaultResponse = { Headers = Map.empty; StatusCode = None; Body = ""; }

    let EmptyRequest = { Headers = Map.empty; Cookies = []; QueryString = Map.empty; Path = System.Uri("http://www.google.com"); User = None }