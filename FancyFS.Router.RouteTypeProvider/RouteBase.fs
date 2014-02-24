namespace FancyFS.Routing

open System.Collections.Generic

type RouteBase(pars:Dictionary<string, obj>) as this =

    [<DefaultValue>] val mutable private routeParameters : Dictionary<string, obj>

    do this.routeParameters <- pars

    new () =
        RouteBase(Dictionary<string, obj>())
