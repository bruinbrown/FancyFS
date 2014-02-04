namespace FancyFS.Core.Tests

open FsUnit
open NUnit.Framework

[<TestFixture>]
module PipelineTests =

    open FancyFS.Core
    open FancyFS.Core.PipelineModule
    open FancyFS.Core.RequestResponseModule

    let SampleRequest = { Headers = Map.empty; QueryString = Map.empty; Cookies = []; Path = System.Uri("http://wwww.google.com"); User = None; }

    [<Test>]
    let ``Adding a function to the pipeline should execute it`` () =
        let writerFunc inp =
            async {
                let! req, resp = inp
                return (req, { resp with Body = "Function executed" })
            }

        let pipeline = writerFunc

        let output = ExecutePipelineAsync pipeline SampleRequest DefaultResponse

        let req, res = Async.RunSynchronously output

        res.Body |> should equal "Function executed"