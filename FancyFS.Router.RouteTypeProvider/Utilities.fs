namespace FancyFS.Routing

[<AutoOpen>]
module Utilities =

    open FancyFS.Routing.RouteParsers

    let mdict (sequence:('a * 'b) seq) =
        let dict = System.Collections.Generic.Dictionary<'a, 'b>()
        sequence
        |> Seq.iter (fun (a, b) -> dict.[a] <- b)
        dict

    let Parse (pattern:string) (arg:string) =
        let d = Seq.zip (pattern.Split('/')) (arg.Split('/'))
                |> Seq.map (fun (f,s) -> f.TrimStart('{').TrimEnd('}'), s)
                |> Seq.map (fun (f,s) -> let q = f.Split(':')
                                         if q.Length = 1 then
                                             f, q.[0] :> obj
                                         else
                                             let convname = q.[1]
                                             let res = ConvertItem s q.[1]
                                             q.[0], res)
                |> mdict
        d

