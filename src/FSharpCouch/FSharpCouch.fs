module FSharpCouch
    open System
    open System.Net
    open System.Text
    open System.IO
    open System.Net
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    type CouchDocument<'a> = {
        id : string
        rev : string
        body : 'a
    }

    let private memoize fn =
        let cache = ref Map.empty
        fun key ->
            let cacheMap = !cache 
            match cacheMap.TryFind key with
            | Some result -> result
            | None ->
                let result = fn key
                cache := Map.add key result cacheMap
                result

    let private writeRequest url methodName contentType content =
        let request = WebRequest.Create(string url)
        request.Method <- methodName
        request.ContentType <- contentType 
        let bytes = UTF8Encoding.UTF8.GetBytes(string content)
        use requestStream = request.GetRequestStream()
        requestStream.Write(bytes, 0, bytes.Length) 
        request
    
    let private asyncReadResponse (request:WebRequest) =
        async { use! response = request.AsyncGetResponse()
                use stream = response.GetResponseStream()
                use reader = new StreamReader(stream)
                let contents = reader.ReadToEnd()
                return contents }
    
    let private processRequest url methodName contentType content =
        match methodName with
        | "POST" -> 
            writeRequest url methodName contentType content
        | _ -> 
            let req = WebRequest.Create(string url)
            req.Method <- methodName
            req
        |> asyncReadResponse 
        |> Async.RunSynchronously
    
    let private toJson content =
        JsonConvert.SerializeObject content
    
    let private fromJson<'a> json =
        JsonConvert.DeserializeObject<'a> json

    let private toResponseObject<'a> response =
        let result = response |> fromJson<'a>
        let json = response |> JObject.Parse
        { id = json.["_id"].ToString(); rev = json.["_rev"].ToString(); body = result }
            
    let private buildUrl (server:string) (database:string) =
        server + "/" + database.ToLower()
    
    let createDatabase server database =
        memoize 
            (fun s d -> 
                try
                    processRequest (buildUrl s d) "PUT" "" "" |> ignore
                with
                | :? WebException as wex -> () // the database already exists
            ) server database  
    
    let deleteDatabase server database =
        createDatabase server database
        processRequest (buildUrl server database) "DELETE" "" "" |> ignore
    
    let createDocument server database content = 
        createDatabase server database
        let response = 
            content |> toJson
            |> processRequest (buildUrl server database) "POST" "application/json"
        let result = response |> fromJson<'a>
        let json = response |> JObject.Parse
        { id = json.["id"].ToString(); rev = json.["rev"].ToString(); body = result }
    
    let getDocument<'a> server database documentId =
        createDatabase server database
        processRequest ((buildUrl server database) + "/" + documentId) "GET" "" ""
        |> toResponseObject<'a>
    
    let getAllDocuments<'a> server database =
        createDatabase server database
        let jsonObject = processRequest ((buildUrl server database) + "/_all_docs?include_docs=true") "GET" "" ""
                         |> JObject.Parse
        Async.Parallel [for row in jsonObject.["rows"] -> 
                            async {
                                return row.["doc"].ToString() |> toResponseObject<'a>
                            }]
        |> Async.RunSynchronously
    
    let deleteDocument server database documentId revision =  
        createDatabase server database       
        processRequest ((buildUrl server database) + "/" + documentId + "?rev=" + revision) "DELETE" "" "" |> ignore
