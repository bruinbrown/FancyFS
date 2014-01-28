namespace FancyFS.ASP.Example

open FancyFS.Core
open FancyFS.Core.App

type ExamplePipelineLocation () =

    let writerFunc = fun (req, res) -> (req, { res with Body = "testing"})

    let pipeline = BaseRequest ==> writerFunc

    interface IPipelineLocation with
        member this.Pipeline
            with get () = pipeline
