namespace FancyFS.Hosting.ASPNET

open System
open System.Web
open System.Configuration

open FancyFS.Core
open FancyFS.Core.App

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

    member this.Pipeline
        with get () = this.["pipeline"] :?> FancyPipelineElement
        and set (v:FancyPipelineElement) = this.["pipeline"] <- (v :> obj)

type FancyHttpRequestHandler () as this = 

    let ConvertRequest (wrapper:HttpContextWrapper) =
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
        { Headers = headers; QueryString = qs; User = user; Path = path; Cookies = cookies; }

    let CreateResponse resp (wrapper:HttpContextWrapper) =
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

    interface IHttpHandler with
        member this.IsReusable 
            with get () = false

        member this.ProcessRequest (context:HttpContext) =
            let wrapper = HttpContextWrapper(context)
            let req = ConvertRequest wrapper
            let writerFunc = fun (req, resp) -> (req, { resp with Body = "test"})
            let pipeline = BaseRequest ==> writerFunc
            let resp = ExecutePipeline pipeline req
            CreateResponse resp wrapper
            ()