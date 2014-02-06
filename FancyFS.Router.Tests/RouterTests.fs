namespace FancyFS.Routing.Tests

open FsUnit
open NUnit.Framework

[<TestFixture>]
module TrieTests =
    
    open FancyFS.Core
    open FancyFS.Core.RequestResponseModule
    open FancyFS.Core.DataStructures
    open FancyFS.Routing
    open FancyFS.Routing.Trie.RouterTrie

    let passFn input =
        Assert.Pass ("Correct function called")
        async {
            return DefaultResponse
        }

    let failFn input =
        Assert.Fail ("Incorrect function called")
        async {
            return DefaultResponse
        }

    let fakeInput = { Request = EmptyRequest; PathVariables = Map.empty }

    [<Test>]
    let ``An empty trie shouldn't return a function`` () =
        let trie = EndOfRoute
        let func = GetRouteFunction trie "GET/test/path"

        func |> should equal None

    [<Test>]
    let ``A trie with paths should return a valid function when requested`` () =
        let exampleFn input =
            async {
                return DefaultResponse
            }
        let routes = ("GET/users/lookup/test", exampleFn) :: ("GET/users/lookup/another", exampleFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/lookup/test"
        match fst func with
        | Some _ -> Assert.Pass()
        | None -> Assert.Fail ("Func was none")

    [<Test>]
    let ``A trie with a path within a path should return the correct function`` () =
        let routes = ("GET/users/slightly/longer/path", failFn) :: ("GET/users/slightly/longer", passFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/slightly/longer"
        match fst func with
        | Some x -> let res = x (fakeInput, DefaultResponse)
                    ()
        | None -> Assert.Fail ("No matching function found")
                  ()

    [<Test>]
    let ``A trie with a shorter path should return the longer path when requested`` () =
        let routes = ("GET/users/slightly/longer/path", passFn) :: ("GET/users/slightly/longer", failFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/slightly/longer/path"
        match fst func with
        | Some x -> let res = x (fakeInput, DefaultResponse)
                    ()
        | None -> Assert.Fail ("No matching function found")
                  ()

    [<Test>]
    let ``2 tries of equal length should return the requested function`` () =
        let routes = ("GET/users/slightly/different", passFn) :: ("GET/users/slightly/longer", failFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/slightly/different"
        match fst func with
        | Some x -> let res = x (fakeInput, DefaultResponse)
                    ()
        | None -> Assert.Fail ("No matching function found")
                  ()