// This is a simple .NET API for CouchDB loosely based on SharpCouch
// http://code.google.com/p/couchbrowse/source/browse/trunk/SharpCouch/SharpCouch.cs
module FSharpCouch
    open System
    open System.Net
    open System.Text
    open System.IO
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    let WriteRequest url methodName content contentType =
        let request = WebRequest.Create(string url)
        request.Method <- methodName
        request.ContentType <- contentType 
        let bytes = UTF8Encoding.UTF8.GetBytes(string content)
        use requestStream = request.GetRequestStream()
        requestStream.Write(bytes, 0, bytes.Length) 
        request
    let AsyncReadResponse (request:WebRequest) =
        async { use! response = request.AsyncGetResponse()
                use stream = response.GetResponseStream()
                use reader = new StreamReader(stream)
                let contents = reader.ReadToEnd()
                return contents }
    let ProcessPostRequest url methodName content contentType =
        WriteRequest url methodName content contentType 
        |> AsyncReadResponse
        |> Async.RunSynchronously
    let ProcessGetPutOrDeleteRequest url methodName =
        let request = WebRequest.Create(string url)
        request.Method <- methodName
        AsyncReadResponse request 
        |> Async.RunSynchronously
//    let ProcessGetRequest url =
//        WebRequest.Create(string url)
//        |> AsyncReadResponse
//        |> Async.RunSynchronously
    let BuildUrl (server:string) (database:string) =
        server + "/" + database.ToLower()
    let CreateDatabase server database =
        ProcessGetPutOrDeleteRequest (BuildUrl server database) "PUT"
    let DeleteDatabase server database =
        ProcessGetPutOrDeleteRequest (BuildUrl server database) "DELETE"
    let CreateDocument server database content = 
        let jsonContent = JsonConvert.SerializeObject content
        ProcessPostRequest (BuildUrl server database) "POST" jsonContent "application/json"
    let GetDocument<'a> server database documentId =
        let response = ProcessGetPutOrDeleteRequest ((BuildUrl server database) + "/" + documentId) "GET"
        JsonConvert.DeserializeObject<'a> response
    let GetAllDocuments<'a> server database =
        let jsonObject = ProcessGetPutOrDeleteRequest ((BuildUrl server database) + "/_all_docs") "GET"
                         |> JObject.Parse
        Async.Parallel [for row in jsonObject.["rows"] -> 
                            async {return JsonConvert.DeserializeObject<'a>(row.ToString())}]
        |> Async.RunSynchronously
    let DeleteDocument server database documentId revision =         
        ProcessGetPutOrDeleteRequest ((BuildUrl server database) + "/" + documentId + "?rev=" + revision) "DELETE"
