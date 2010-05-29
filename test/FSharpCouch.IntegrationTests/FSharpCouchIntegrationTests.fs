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
        val mutable getDocumentResult : TestExistingRecord
        [<DefaultValue(false)>]
        val mutable fakeRecord : TestRecord
        let CouchDbServer = "http://localhost:5984"
        let Database = "FSharpCouchTest"
        inherit SpecUnit.ContextSpecification()
            override x.Because () =
                x.fakeRecord <- {_id = Guid.NewGuid().ToString(); SomeValue = "Test"}
                try
                    FSharpCouch.CreateDatabase CouchDbServer Database |> ignore
                    FSharpCouch.CreateDocument CouchDbServer Database x.fakeRecord |> ignore 
                    x.getDocumentResult <- FSharpCouch.GetDocument<TestExistingRecord> CouchDbServer Database x.fakeRecord._id
                    System.Threading.Thread.Sleep(100)
                    FSharpCouch.DeleteDocument CouchDbServer Database x.fakeRecord._id x.getDocumentResult._rev |> ignore
                finally
                    System.Threading.Thread.Sleep(100)
                    FSharpCouch.DeleteDatabase CouchDbServer Database |> ignore
            [<Test>]    
            member x.should_have_a_document_with_the_expected_id () =    
                x.getDocumentResult._id.ShouldEqual x.fakeRecord._id |> ignore

    [<TestFixture>]      
    type FSharpCouch__when_getting_multiple_documents () =   
        [<DefaultValue(false)>]  
        val mutable getDocumentsResult : TestExistingRecord seq
        [<DefaultValue(false)>]
        val mutable fakeRecord : TestRecord
        let CouchDbServer = "http://localhost:5984"
        let Database = "FSharpCouchTest"
        inherit SpecUnit.ContextSpecification()
            override x.Because () =
                x.fakeRecord <- {_id = Guid.NewGuid().ToString(); SomeValue = "Test"}
                let fakeRecord2 = {x.fakeRecord with _id = Guid.NewGuid().ToString()}
                let fakeRecord3 = {x.fakeRecord with _id = Guid.NewGuid().ToString()}
                try
                    FSharpCouch.CreateDatabase CouchDbServer Database |> ignore
                    FSharpCouch.CreateDocument CouchDbServer Database x.fakeRecord |> ignore 
                    FSharpCouch.CreateDocument CouchDbServer Database fakeRecord2 |> ignore 
                    FSharpCouch.CreateDocument CouchDbServer Database fakeRecord3 |> ignore 
                    let documentIds = [x.fakeRecord._id; fakeRecord2._id; fakeRecord3._id]
                    x.getDocumentsResult <- FSharpCouch.GetDocuments<TestExistingRecord> CouchDbServer Database documentIds
                finally
                    System.Threading.Thread.Sleep(200)
                    FSharpCouch.DeleteDatabase CouchDbServer Database |> ignore
            [<Test>]    
            member x.should_have_3_documents () =    
                (Seq.length x.getDocumentsResult).ShouldEqual 3 |> ignore

    [<TestFixture>]      
    type FSharpCouch__when_getting_all_documents () =   
        [<DefaultValue(false)>]  
        val mutable getDocumentsResult : TestExistingRecord seq
        [<DefaultValue(false)>]
        val mutable fakeRecord : TestRecord
        let CouchDbServer = "http://localhost:5984"
        let Database = "FSharpCouchTest"
        inherit SpecUnit.ContextSpecification()
            override x.Because () =
                x.fakeRecord <- {_id = Guid.NewGuid().ToString(); SomeValue = "Test"}
                let fakeRecord2 = {x.fakeRecord with _id = Guid.NewGuid().ToString()}
                let fakeRecord3 = {x.fakeRecord with _id = Guid.NewGuid().ToString()}
                try 
                    FSharpCouch.CreateDatabase CouchDbServer Database |> ignore
                    FSharpCouch.CreateDocument CouchDbServer Database x.fakeRecord |> ignore 
                    FSharpCouch.CreateDocument CouchDbServer Database fakeRecord2 |> ignore 
                    FSharpCouch.CreateDocument CouchDbServer Database fakeRecord3 |> ignore 
                    x.getDocumentsResult <- FSharpCouch.GetAllDocuments<TestExistingRecord> CouchDbServer Database
                finally
                    System.Threading.Thread.Sleep(100)
                    FSharpCouch.DeleteDatabase CouchDbServer Database |> ignore
            [<Test>]    
            member x.should_have_3_documents () =    
                (Seq.length x.getDocumentsResult).ShouldEqual 3 |> ignore
