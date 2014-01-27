namespace FancyFS.Core

open FsUnit
open NUnit.Framework

[<TestFixture>]
module AppTests =

    open FancyFS.Core
    open FancyFS.Core.App

    let private PipelineLength pipeline =
        let rec Count pipeline count =
            match pipeline.Next with
            | None -> count
            | Some x -> Count x (count+1)
        Count pipeline 0

    [<Test>]
    let ``Using ==> should increase pipeline length`` () =
        let testFun = fun (req, resp) -> (req, resp)

        let emptyPipeline = BaseRequest

        let newPipeline = emptyPipeline ==> testFun

        (PipelineLength emptyPipeline) |> should equal 0
        (PipelineLength newPipeline) |> should equal 1