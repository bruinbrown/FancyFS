namespace FancyFS.Routing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open ProviderImplementation.ProvidedTypes
open FancyFS.Routing
open FancyFS.Routing.RouteParsers

[<TypeProvider>]
type RouteTypeProvider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    
    let ns = "FancyFS.Routing"
    let asm = Assembly.GetExecutingAssembly()
    let baseType = typeof<RouteBase>
    let staticParams = [ProvidedStaticParameter("routeDefinition", typeof<string>)]

    let routeType = ProvidedTypeDefinition(asm, ns, "RouteProvider", None)

    do routeType.DefineStaticParameters(
        parameters = staticParams,
        instantiationFunction = (fun typeName parameterValues -> 
            match parameterValues with
            | [| :? string as pattern |] ->
                let parameters = pattern.Split('/')
                                 |> List.ofArray
                                 |> List.filter (fun t -> t.StartsWith("{") && t.EndsWith("}"))
                                 |> List.map (fun t -> t.TrimStart('{').TrimEnd('}'))
                                 |> List.map (fun t -> let q = t.Split(':')
                                                       if q.Length = 1 then
                                                           q.[0], typeof<string>
                                                       else
                                                           let converterType = (GetConverter q.[1]).MatchType()
                                                           q.[0], converterType)
                                 |> List.map (fun (value, typestring) -> ProvidedProperty(value, typestring,
                                                                              GetterCode = (fun [arg1] -> let valExpr = <@@ let inst = (%%arg1 : RouteBase)
                                                                                                                            let backingDict = inst.GetType().GetField("routeParameters", BindingFlags.NonPublic ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Instance)
                                                                                                                            let dictValue = backingDict.GetValue(inst) :?> Dictionary<string, obj>
                                                                                                                            dictValue.[value] @@>
                                                                                                          Expr.Coerce(valExpr, typestring)),
                                                                              SetterCode = (fun [arg1; arg2] -> let valExpr = <@@ let inst = (%%arg1 : RouteBase)
                                                                                                                                  let backingDict = inst.GetType().GetField("routeParameters", BindingFlags.NonPublic ||| BindingFlags.FlattenHierarchy ||| BindingFlags.Instance)
                                                                                                                                  let dictValue = backingDict.GetValue(inst) :?> Dictionary<string, obj>
                                                                                                                                  let setValue = (%%arg2 :> obj)
                                                                                                                                  dictValue.[value] <- setValue @@>
                                                                                                                valExpr)))

                let ty = ProvidedTypeDefinition(asm, ns, typeName, Some baseType)
                ty.AddMember(ProvidedMethod("Path", [], typeof<string>,
                                                InvokeCode = (fun exprs -> <@@ pattern @@>),
                                                IsStaticMethod = true))

                let cons = ProvidedConstructor([], InvokeCode = fun args -> <@@ RouteBase() @@>)
                let cons1 = ProvidedConstructor([ProvidedParameter("routeParameters", typeof<IDictionary<string, obj>>)],
                                                InvokeCode = (fun [arg1] -> <@@ let dic = %%arg1 : IDictionary<string, obj>
                                                                                RouteBase(dic) @@>))

                let invokeCode (parameters:Expr list) =
                    <@@ let str = (%%(parameters.[1]) : string)
                        str @@>

                let parseMethod = ProvidedMethod(methodName = "Parse",
                                                 parameters = [ ProvidedParameter("destination", typeof<string>) ],
                                                 returnType = typeof<RouteBase>,
                                                 IsStaticMethod = true,
                                                 InvokeCode = (fun [arg1] -> let par = <@@ let arg = (%%arg1 : string)
                                                                                           (Parse pattern arg) :> IDictionary<string, obj> @@>
                                                                             Expr.NewObject(cons1, [par])))

                ty.AddMembers([cons; cons1])
                ty.AddMembers(parameters)
                ty.AddMember(parseMethod)
                ty
            | _ -> failwith "Unexpected number of parameters"))
    do this.AddNamespace(ns, [routeType])

[<assembly:TypeProviderAssembly>]
do()