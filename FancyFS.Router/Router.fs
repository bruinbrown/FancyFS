namespace FancyFS.Routing

open FancyFS.Core
open FancyFS.Core.PipelineModule

module Router =
    
    open System.Collections.Generic
    open FancyFS.Routing.Trie
    open FancyFS.Routing.Trie.RouterTrie
    open System.Net

    let private definedRoutes = List<string * ((Input * Response) -> Async<Response>)>()

    let private CreateTrie () =
        let t = definedRoutes
                |> List.ofSeq
        let trie = CreateTrie t
        trie

    let HandleInput trie input =
        let GetFunction = GetRouteFunction trie
        let res = async {
            let! req, resp = input
            let meth = req.RequestMethod
            let dictkey = req.RequestMethod.ToString() + req.Path.AbsolutePath
            
            let fn, routeVariables = GetFunction dictkey
            let inp = { PathVariables = routeVariables; Request = req }
            let result = match fn with
                         | Some x -> x (inp, FancyFS.Core.RequestResponseModule.DefaultResponse)
                         | None -> async { return { resp with StatusCode = Some HttpStatusCode.NotFound } }
            let! result = result
            return (req, result)
        }
        res

    let GetPipelineElement () =
        (* Here we'll need to initialise all static constructors which reference CreateRoute, otherwise
           we won't have anything in definedRoutes *)
        let trie = CreateTrie ()
        HandleInput trie

    let CreateRoute (baseRoute:string) (httpMethod:RequestMethod) (route:string) (fn:(Input * Response) -> Async<Response>) =
        let reqMethodStr = httpMethod.ToString()
        //TODO: Change this to a safer way of combining paths
        let route = baseRoute + route
        let fullPath = if route.StartsWith("/") then
                           String.concat "" [| reqMethodStr; route|]
                       else
                           String.concat "/" [| reqMethodStr; route|]
        let routeDefinition = (fullPath, fn)
        definedRoutes.Add(routeDefinition)
        ()
