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

(* I'm not a fan of how I've handled this here. The only pure way would take significantly longer and blow up the stack
   Open to any suggestions on how this could be changed to be implemented better *)
type Pipeline =
    {
        Element : (Async<(Request * Response)> -> Async<(Request * Response)>)
        mutable Next : Pipeline option
    }

type IPipelineLocation =
    abstract member Pipeline : Pipeline with get

module App =
    
    let private defaultResponse = { Headers = Map.empty; StatusCode = None; Body = ""; }

    let BaseRequest =
        let func inp = inp
        { Element = func; Next = None; } 

    (* As above. Not keen on mutable but for now, it will have to do*)
    let (==>) pipeline func =
        let rec GetLastElem elem =
            match elem.Next with
            | Some x -> GetLastElem x
            | None -> elem
        let last = GetLastElem pipeline
        do last.Next <- Some { Element = func; Next = None; }
        pipeline

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

    let ExecutePipelineAsync pipeline request =
        let rec ExecuteElement input elem =
            let p = elem.Element input
            match elem.Next with
            | Some x -> ExecuteElement p x
            | None -> p
        let asyncInput =
            async {
                return (request, defaultResponse)
            }
        ExecuteElement asyncInput pipeline 