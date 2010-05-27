// This is a partial port of SharpCouch
// http://code.google.com/p/couchbrowse/source/browse/trunk/SharpCouch/SharpCouch.cs
module FSharpCouch
    open System
    open System.Net
    open System.Text
    open System.IO
    open Newtonsoft.Json

    let WriteRequest url methodName content contentType =
        let request = WebRequest.Create(string url)
        request.Method <- methodName
        request.ContentType <- contentType 
        let bytes = UTF8Encoding.UTF8.GetBytes(string content)
        use requestStream = request.GetRequestStream()
        requestStream.Write(bytes, 0, bytes.Length) 
        request
    let AyncReadResponse (request:WebRequest) =
        async { use! response = request.AsyncGetResponse()
                use stream = response.GetResponseStream()
                use reader = new StreamReader(stream)
                let contents = reader.ReadToEnd()
                return contents }
    let ProcessWriteRequest url methodName content contentType =
        async { let request = WriteRequest url methodName content contentType
                let! response = AyncReadResponse(request) 
                return response}
        |> Async.RunSynchronously
    let ProcessReadRequest url =
        async { let request = WebRequest.Create(string url)
                let! response = AyncReadResponse(request) 
                return response}
        |> Async.RunSynchronously
    let BuildUrl server database =
        server + "/" + database 
    let CreateDocument server database content = 
        let jsonContent = JsonConvert.SerializeObject content
        ProcessWriteRequest (BuildUrl server database) "POST" jsonContent "application/json"
    let GetDatabases server =
        let response = ProcessReadRequest (server + "/_all_dbs")
        JsonConvert.DeserializeObject response 
    let GetAllDocuments server database = 
        let response = ProcessReadRequest (BuildUrl server database) + "/_all_docs"
        JsonConvert.DeserializeObject response 
    let CreateDatabase server database =
        let response = ProcessWriteRequest (BuildUrl server database) "PUT" "" "application/json"
        match response with 
        | _ when response = "{\"ok\":true}" -> failwith "Failed to create the database"
        | _ -> response
    let DeleteDatabase server database =
        let response = ProcessWriteRequest (BuildUrl server database) "DELETE" "" "application/json"
        match response with 
        | _ when response = "{\"ok\":true}" -> failwith "Failed to delete the database"
        | _ -> response
     