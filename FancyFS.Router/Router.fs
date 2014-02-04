namespace FancyFS.Routing

open FancyFS.Core
open FancyFS.Core.PipelineModule

module Router =
    
    open System.Collections.Generic

    let private definedRoutes = List<(string * (Async<Request * Response> -> Async<Request * Response>))>()

    let private CreateTrie routes =
        ()

    let public GetPipelineElement () =
        let Func input =
            async {
                let! req, resp = input
                let routeTrie = CreateTrie definedRoutes
                return (req, resp)
            }
        Func

    let CreateRoute httpMethod (route:string) (fn:Async<Request * Response> -> Async<Request * Response>) =
        let fullPath = if route.StartsWith("/") then
                           String.concat "" [| httpMethod; route|]
                       else
                           String.concat "/" [| httpMethod; route|]
        let routeDefinition = (fullPath, fn)
        definedRoutes.Add(routeDefinition)
