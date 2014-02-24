namespace FancyFS.Routing

open System.Collections.Generic

type RouteBase(pars:IDictionary<string, obj>) as this =

    [<DefaultValue>] val mutable private routeParameters : IDictionary<string, obj>

    do this.routeParameters <- pars

    new () =
        RouteBase(Dictionary<string, obj>())
