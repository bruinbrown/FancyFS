namespace FancyFS.ASP.Example

open FancyFS.Core
open FancyFS.Core.RequestResponseModule
open FancyFS.Core.PipelineModule

type ExamplePipelineLocation () =

    let writerFunc input = 
        async {
            let! (req, resp) = input
            return (req, { resp with Body = resp.Body + "testing\n" })
        }

    let delayFunc input =
        (async {
            let! req, resp = input
            do! Async.Sleep(10000)
            return (req, resp)
        })
        
    let pipeline = writerFunc ==> delayFunc ==> writerFunc

    interface IPipelineLocation with
        member this.Pipeline
            with get () = pipeline
