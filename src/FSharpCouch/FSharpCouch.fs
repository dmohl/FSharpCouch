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
    let ProcessPostRequest url methodName content contentType =
        async { let request = WriteRequest url methodName content contentType
                let! response = AyncReadResponse(request) 
                return response}
        |> Async.RunSynchronously
    let ProcessPutOrDeleteRequest url methodName =
        async { let request = WebRequest.Create(string url)
                request.Method <- methodName
                let! response = AyncReadResponse(request) 
                return response}
        |> Async.RunSynchronously
    let ProcessGetRequest url =
        async { let request = WebRequest.Create(string url)
                let! response = AyncReadResponse(request) 
                return response}
        |> Async.RunSynchronously
    let BuildUrl (server:string) (database:string) =
        server + "/" + database.ToLower() 
    let CreateDatabase server database =
        try
            ProcessPutOrDeleteRequest (BuildUrl server database) "PUT"
        with
        | e -> failwith e.Message
    let DeleteDatabase server database =
        try
            ProcessPutOrDeleteRequest (BuildUrl server database) "DELETE"
        with
        | e -> failwith e.Message
    let CreateDocument server database content = 
        let jsonContent = JsonConvert.SerializeObject content
        ProcessPostRequest (BuildUrl server database) "POST" jsonContent "application/json"
    let GetDocument<'a> server database documentId =
        let response = ProcessGetRequest ((BuildUrl server database) + "/" + documentId) 
        JsonConvert.DeserializeObject(response, typeof<'a>) :?> 'a
    let GetDocuments<'a> server database documentIds =
        Async.Parallel [for documentId in documentIds -> 
                            async {return GetDocument<'a> server database documentId }]
        |> Async.RunSynchronously
    let DeleteDocument server database documentId revision =         
        try
            ProcessPutOrDeleteRequest ((BuildUrl server database) + "/" + documentId + "?rev=" + revision) "DELETE"
        with
        | e -> failwith e.Message
