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
    let ``BaseRequest should cause pipeline count to remain at zero`` () =
        let emptyPipeline = BaseRequest
        (PipelineLength emptyPipeline) |> should equal 0

    [<Test>]
    let ``Using ==> should increase pipeline length`` () =
        let testFun = fun inp -> inp
        let newPipeline = BaseRequest ==> testFun
        (PipelineLength newPipeline) |> should equal 1

    [<Test>]
    let ``Using multiple ==> should make pipeline to be same length`` () =
        let testFun = fun inp -> inp
        do BaseRequest.Next <- None
        let newPipeline = BaseRequest ==> testFun ==> testFun ==> testFun ==> testFun
        (PipelineLength newPipeline) |> should equal 4
