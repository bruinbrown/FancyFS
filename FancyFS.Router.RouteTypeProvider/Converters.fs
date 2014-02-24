namespace FancyFS.Routing.RouteParsers

type IRouteParser =
    abstract member MatchType : unit -> System.Type
    abstract member Parse : string -> obj
    abstract member Name : unit -> string

type IntRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<int>

        member this.Parse value =
            System.Int32.Parse(value) :> obj

        member this.Name () = "int"

type DateTimeRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<System.DateTime>

        member this.Parse value =
            System.DateTime.Parse(value) :> obj

        member this.Name () = "datetime"

type GuidRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<System.Guid>

        member this.Parse value =
            System.Guid.Parse(value) :> obj

        member this.Name () = "guid"

type FloatRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<float>

        member this.Parse value =
            System.Double.Parse(value) :> obj

        member this.Name () = "float"

[<AutoOpen>]
module RouteParserUtilities =
    open System.Reflection
    open System

    let private routeParsers = Assembly.GetExecutingAssembly().GetTypes()
                               |> Seq.filter (fun t -> t.GetInterface("IRouteParser") <> null)
                               |> Seq.map (fun t -> Activator.CreateInstance(t) :?> IRouteParser)
                               |> List.ofSeq

    let AvailableRouteParsers = routeParsers

    let GetConverter converterName =
        routeParsers
        |> List.filter (fun t -> t.Name() = converterName)
        |> List.head

    let ConvertItem string converterName =
        (GetConverter converterName).Parse(string)

    let GetTypeForPathSegment (path:string) =
        let q = path.Split(':')
        if q.Length = 1 then
            q.[0], typeof<string>
        else
            let p = GetConverter (q.[1])
            q.[0], p.MatchType()