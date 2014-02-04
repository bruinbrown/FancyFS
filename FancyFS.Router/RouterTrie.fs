namespace FancyFS.Routing.Trie

open System.Text
open FancyFS.Core
open FancyFS.Core.DataStructures

type TrieNodeFunction = Async<Request * Response> -> Async<Request * Response>

module RouterTrie =

    let CreateTrie (routeDefinitions:seq<string * string * TrieNodeFunction>) =
        let rec generateTrieNode routeDefns =
            let isSomeFunction = routeDefns
                                 |> Seq.filter (fun (path, _) -> path = [])
                                 |> (fun t -> if not (Seq.isEmpty t) then Some (t |> Seq.head |> snd) else None)

            let nextRouteFragments = routeDefns
                                     |> Seq.filter (fun (path, _) -> path <> [])
                                     |> Seq.groupBy (fun (path, _) -> path |> Seq.head)
                                     |> Seq.map (fun (basePath, rest) -> let otherRoutes = rest |> Seq.map (fun t -> t |> fst |> List.tail, snd t)
                                                                         basePath, otherRoutes)
                                     |> Seq.map (fun (basePath, routeDefn) -> basePath, generateTrieNode routeDefn)
                                     |> Seq.fold (fun acc (basePath, trieNode) ->
                                                      (basePath, trieNode) :: acc) []

            Node(dict nextRouteFragments, isSomeFunction)
        
        let definitions = routeDefinitions
                          |> Seq.map (fun (httpMethod, route, fn) -> let path = if route.StartsWith("/") then 
                                                                                    (sprintf "%s%s" httpMethod route)
                                                                                else
                                                                                    (sprintf "%s/%s" httpMethod route)
                                                                     path.Split('/') |> List.ofArray, fn)
        generateTrieNode definitions

    let GetRouteFunction trie (route:string) =
        let rec getFunction trie routeFragments =
            match routeFragments with
            | [] -> match trie with
                    | Node (_, fn) -> fn
                    | _ -> None
            | x :: xs -> match trie with
                         | Node (dict, _) -> if dict.ContainsKey(x) then
                                                 getFunction dict.[x] xs
                                             else
                                                 None
                         | _ -> None
        getFunction trie (route.Split('/') |> List.ofArray)

