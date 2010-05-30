module FSharpCouchIntegrationTests
    open FSharpCouch
    open System
    open System.Collections.Generic
    open NUnit.Framework
    open SpecUnit

    type TestRecord =
        { _id : string
          SomeValue : string }
    
    type TestExistingRecord =
        { _id : string
          _rev : string
          SomeValue : string }

    [<TestFixture>]      
    type FSharpCouch__when_testing_full_database_and_document_cylce () =   
        [<DefaultValue(false)>]  
        val mutable getDocumentsResult : TestExistingRecord seq
        [<DefaultValue(false)>]  
        val mutable getAllDocumentsResult : TestExistingRecord seq
        [<DefaultValue(false)>]  
        val mutable getDocumentResult : TestExistingRecord
        [<DefaultValue(false)>]
        val mutable fakeRecord : TestRecord
        let CouchDbServer = "http://localhost:5984"
        let Database = "FSharpCouchTest"
        let ProcessCommands commands =
            Async.Parallel [for command in commands -> 
                                async { command |> ignore } ]                          
            |> Async.RunSynchronously |> ignore
        let KillDatabase () =
            try
                DeleteDatabase CouchDbServer Database |> ignore
            with
            | _ -> "do nothing" |> ignore
        [<TestFixtureTearDown>]
        member x.CleanUpContext () = KillDatabase()
        [<TestFixtureSetUp>]
        member x.Context () =
            KillDatabase()
            x.fakeRecord <- {_id = Guid.NewGuid().ToString(); SomeValue = "Test"}
            let fakeRecord2 = {x.fakeRecord with _id = Guid.NewGuid().ToString()}
            let fakeRecord3 = {x.fakeRecord with _id = Guid.NewGuid().ToString()} 
            CreateDatabase CouchDbServer Database |> ignore
            let createCommands = 
                [ CreateDocument CouchDbServer Database x.fakeRecord
                  CreateDocument CouchDbServer Database fakeRecord2
                  CreateDocument CouchDbServer Database fakeRecord3 ]
            ProcessCommands createCommands
            let getCommands = 
                [ x.getDocumentResult <- 
                      GetDocument<TestExistingRecord> CouchDbServer Database x.fakeRecord._id
                  x.getAllDocumentsResult <- 
                      GetAllDocuments<TestExistingRecord> CouchDbServer Database ]
            ProcessCommands getCommands
            DeleteDocument CouchDbServer Database x.fakeRecord._id x.getDocumentResult._rev |> ignore
        [<Test>]    
        member x.should_have_a_document_with_the_expected_id () =    
            x.getDocumentResult._id.ShouldEqual x.fakeRecord._id |> ignore
        [<Test>]    
        member x.should_have_3_documents_from_get_all () =    
            (Seq.length x.getAllDocumentsResult).ShouldEqual 3 |> ignore
