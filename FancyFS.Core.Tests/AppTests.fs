namespace FancyFS.Core.Tests

open FsUnit
open NUnit.Framework

[<TestFixture>]
module PipelineTests =

    open FancyFS.Core
    open FancyFS.Core.PipelineModule
    open FancyFS.Core.RequestResponseModule

    
    [<Test>]
    let ``Adding a function to the pipeline should execute it`` () =
        let writerFunc inp =
            async {
                let! req, resp = inp
                return (req, { resp with Body = "Function executed" })
            }

        let pipeline = writerFunc

        let output = ExecutePipelineAsync pipeline EmptyRequest DefaultResponse

        let req, res = Async.RunSynchronously output

        res.Body |> should equal "Function executed"