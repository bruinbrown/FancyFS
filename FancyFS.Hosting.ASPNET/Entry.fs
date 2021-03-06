﻿namespace FancyFS.Hosting.ASPNET

open System
open System.Threading.Tasks
open System.Web
open System.Configuration

open FancyFS.Core
open FancyFS.Core.RequestResponseModule
open FancyFS.Core.PipelineModule

type FancyPipelineElement () = 
        inherit ConfigurationElement ()

        [<ConfigurationProperty("type", DefaultValue = "", IsRequired = true)>]
        member this.Type
            with get () = this.["type"] :?> string
            and set (v:string) = this.["type"] <- (v :> obj)

        [<ConfigurationProperty("assembly", DefaultValue = "", IsRequired = true)>]
        member this.Assembly
            with get () = this.["assembly"] :?> string
            and set (v:string) = this.["assembly"] <- (v :> obj)


type FancyConfigurationSection () =
    inherit ConfigurationSection()

    [<ConfigurationProperty("pipeline")>]
    member this.Pipeline
        with get () = this.["pipeline"] :?> FancyPipelineElement
        and set (v:FancyPipelineElement) = this.["pipeline"] <- (v :> obj)

module RequestResponseMappers =
    
    let ConvertRequest (wrapper:HttpContextBase) =
        let rec ToMap (collection:Collections.Specialized.NameValueCollection) (dict:Map<string, string>) index =
            if index = collection.Count then dict
            else
                let key = collection.AllKeys.[index]
                let value = collection.[index]
                let dict = dict.Add(key, value)
                ToMap collection dict (index+1)
          
        let rec ConvertCookies (cookies:HttpCookieCollection) (newCookies:Cookie list) index =
            if index = cookies.Count then newCookies
            else
                let value = cookies.[index]
                let subCookies = ToMap value.Values Map.empty 0
                let c = { Name = value.Name; Value = value.Value; Path = value.Path; Domain = value.Domain; Expires = value.Expires; Secure = value.Secure; Shareable = value.Shareable; SubKeys = subCookies; }
                ConvertCookies cookies (c::newCookies) (index+1)
        
        let headers = ToMap wrapper.Request.Headers Map.empty 0
        let qs = ToMap wrapper.Request.QueryString Map.empty 0
        let user = None
        let path = wrapper.Request.Url
        let cookies = ConvertCookies wrapper.Request.Cookies [] 0
        let reqMethod = RequestMethod.Parse(typeof<RequestMethod>, wrapper.Request.HttpMethod) :?> RequestMethod
        { Headers = headers; QueryString = qs; User = user; Path = path; Cookies = cookies; RequestMethod = reqMethod }


    let CreateResponse (resp:Response) (wrapper:HttpContextBase) =
        let rec AddHeaders (headers:(string * string) list) =
            match headers with
            | [] -> ()
            | x :: xs -> wrapper.Response.AddHeader((fst x), (snd x))
                         AddHeaders xs
        
        AddHeaders (resp.Headers |> Map.toList)
        match resp.StatusCode with
        | Some x -> wrapper.Response.StatusCode <- (int x)
        | None -> wrapper.Response.StatusCode <- 500
        wrapper.Response.Write(resp.Body)

module PipelineLocators =
    
    let GetPipelineFromAssembly () =
        FindPipeline ()

    let GetPipelineFromConfig () =
        try
            let configSection = (ConfigurationManager.GetSection("fancyfs") :?> FancyConfigurationSection)
            let assemblyStr = configSection.Pipeline.Assembly
            let typeStr =  configSection.Pipeline.Type
            let fullname = String.Concat(typeStr, ", ", assemblyStr)
            let t = Type.GetType(fullname)
            let inst = Activator.CreateInstance(t) :?> IPipelineLocation
            Some inst
        with
        | exn -> None

    let GetPipelineLocation () =
        match GetPipelineFromConfig () with
        | Some x -> x
        | None -> GetPipelineFromAssembly()

    let PipelineLocation = GetPipelineLocation()