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

    [<Test>]
    let ``An empty trie shouldn't return a function`` () =
        let trie = EndOfRoute
        let func = GetRouteFunction trie "GET/test/path"

        func |> should equal None

    [<Test>]
    let ``A trie with paths should return a valid function when requested`` () =
        let exampleFn input =
            input
        let routes = ("GET", "/users/lookup/test", exampleFn) :: ("GET", "/users/lookup/another", exampleFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/lookup/test"
        match func with
        | Some _ -> Assert.Pass()
        | None -> Assert.Fail ("Func was none")

    [<Test>]
    let ``A trie with a path within a path should return the correct function`` () =
        let passFn input =
            Assert.Pass ("Correct function called")
            input

        let failFn input =
            Assert.Fail ("Incorrect function called")
            input

        let fakeInput = async {
            return EmptyRequest, DefaultResponse
        }

        let routes = ("GET", "users/slightly/longer/path", failFn) :: ("GET", "users/slightly/longer", passFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/slightly/longer"
        match func with
        | Some x -> let res = x fakeInput
                    ()
        | None -> Assert.Fail ("No matching function found")
                  ()

    [<Test>]
    let ``A trie with a shorter path should return the longer path when requested`` () =
        let passFn input =
            Assert.Pass ("Correct function called")
            input

        let failFn input =
            Assert.Fail ("Incorrect function called")
            input

        let fakeInput = async {
            return EmptyRequest, DefaultResponse
        }

        let routes = ("GET", "users/slightly/longer/path", passFn) :: ("GET", "users/slightly/longer", failFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/slightly/longer/path"
        match func with
        | Some x -> let res = x fakeInput
                    ()
        | None -> Assert.Fail ("No matching function found")
                  ()

    [<Test>]
    let ``2 tries of equal length should return the requested function`` () =
        let passFn input =
            Assert.Pass ("Correct function called")
            input

        let failFn input =
            Assert.Fail ("Incorrect function called")
            input

        let fakeInput = async {
            return EmptyRequest, DefaultResponse
        }

        let routes = ("GET", "users/slightly/different", passFn) :: ("GET", "users/slightly/longer", failFn) :: []
        let trie = CreateTrie routes
        let func = GetRouteFunction trie "GET/users/slightly/different"
        match func with
        | Some x -> let res = x fakeInput
                    ()
        | None -> Assert.Fail ("No matching function found")
                  ()