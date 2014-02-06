namespace FancyFS.Routing

open FancyFS.Core

type Input = {
    PathVariables : Map<string, string>

    Request : Request
}

namespace FancyFS.Routing.Trie

open System.Text
open FancyFS.Core
open FancyFS.Core.DataStructures
open FancyFS.Routing

type TrieNodeFunction = (Input * Response) -> Async<Response>

module RouterTrie =

    let CreateTrie (routeDefinitions:seq<string * TrieNodeFunction>) =
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
                          |> Seq.map (fun (route, fn) -> route.Split('/') |> List.ofArray, fn)
        generateTrieNode definitions

    let GetRouteFunction trie (route:string) =
        let rec getFunction trie routeFragments routeVariables =
            match routeFragments with
            | [] -> match trie with
                    | Node (_, fn) -> fn, routeVariables
                    | _ -> None, Map.empty
            | x :: xs -> match trie with
                         | Node (dict, _) -> if dict.ContainsKey(x) then
                                                 getFunction dict.[x] xs routeVariables
                                             else
                                                 None, Map.empty
                         | _ -> None, Map.empty
        getFunction trie (route.Split('/') |> List.ofArray) Map.empty

