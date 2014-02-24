namespace FancyFS.Routing.RouteParsers

type IRouteParser =
    abstract member MatchType : unit -> System.Type
    abstract member Parse : string -> obj
    abstract member Name : unit -> string
    abstract member CanParse : string -> bool

type IntRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<int>

        member this.Parse value =
            System.Int32.Parse(value) :> obj

        member this.Name () = "int"

        member this.CanParse value =
            System.Int32.TryParse(value) |> fst

type DateTimeRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<System.DateTime>

        member this.Parse value =
            System.DateTime.Parse(value) :> obj

        member this.Name () = "datetime"

        member this.CanParse value =
            System.DateTime.TryParse(value) |> fst

type GuidRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<System.Guid>

        member this.Parse value =
            System.Guid.Parse(value) :> obj

        member this.Name () = "guid"

        member this.CanParse value =
            System.Guid.TryParse(value) |> fst

type FloatRouteParser () =
    interface IRouteParser with
        member this.MatchType () = typeof<float>

        member this.Parse value =
            System.Double.Parse(value) :> obj

        member this.Name () = "float"

        member this.CanParse value =
            System.Double.TryParse(value) |> fst

[<AutoOpen>]
module RouteParserUtilities =
    open System.Reflection
    open System

    let private routeParsers = Assembly.GetExecutingAssembly().GetTypes()
                               |> Seq.filter (fun t -> t.GetInterface("IRouteParser") <> null)
                               |> Seq.map (fun t -> Activator.CreateInstance(t) :?> IRouteParser)
                               |> List.ofSeq

    let AvailableRouteParsers = routeParsers

    let GetConverterName (pathSegment:string) =
        let parts = pathSegment.TrimStart('{').TrimEnd('}').Split(':')
        if parts.Length = 1 then
            None
        else Some parts.[1]

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

    let IsParseable routeSegment value =
        let converterName = GetConverterName routeSegment
        match converterName with
        | None -> true
        | Some x -> let converter = GetConverter x
                    converter.CanParse(value)