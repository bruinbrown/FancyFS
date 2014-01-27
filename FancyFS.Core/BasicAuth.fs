namespace FancyFS.Auth

open FancyFS.Core

type IUserValidation =
    abstract member GetUserFromUsernamePassword : string -> string -> IUser option

module BasicAuth =

    let private DecodeBase64 base64 =
        let data = System.Convert.FromBase64String(base64)
        let str = System.Text.Encoding.ASCII.GetString(data)
        let split = str.Split(':')
        (split.[0], split.[1])

    let PipelineElement (userValidation:IUserValidation) (req:Request, resp) =
        let userData = req.Headers.TryFind "Authorization"
        match userData with
        | Some x -> let base64 = (x.Split(' ')).[1]
                    let user,pass = DecodeBase64 base64
                    let user = userValidation.GetUserFromUsernamePassword user pass
                    let req = { req with User = user; }
                    (req, resp)
        | None -> (req, resp)