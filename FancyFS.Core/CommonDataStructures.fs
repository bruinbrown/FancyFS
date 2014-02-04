namespace FancyFS.Core.DataStructures

open System.Collections.Generic

type TrieNode<'a> =
    | Node of IDictionary<string, TrieNode<'a>> * 'a
    | EndOfRoute
