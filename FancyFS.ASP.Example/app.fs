namespace FancyFS.ASP.Example

open FancyFS.Core
open FancyFS.Core.RequestResponseModule
open FancyFS.Core.PipelineModule
open FancyFS.Routing
open FancyFS.Routing.Router

module UsersModule =
    
    let CreateUsersRoute = CreateRoute "/users"

    let GetUsersData = fun (input, response) -> async { return { response with Body = "GetUsersData" } }

    let GetMoreStuff = fun (input, response) -> async { return { response with Body = "GetMoreStuff" } }

    let GetPost = fun (input, response) -> async { return { response with Body = "This was a POST"; StatusCode = Some System.Net.HttpStatusCode.OK } }

    do CreateUsersRoute RequestMethod.GET "/data" GetUsersData

    do CreateUsersRoute RequestMethod.GET "/stuff" GetMoreStuff

    do CreateUsersRoute RequestMethod.POST "/data" GetPost

    do ()

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
     
    let c = UsersModule.CreateUsersRoute
        
    let routerElement = GetPipelineElement ()

    let pipeline = routerElement

    interface IPipelineLocation with
        member this.Pipeline
            with get () = pipeline
