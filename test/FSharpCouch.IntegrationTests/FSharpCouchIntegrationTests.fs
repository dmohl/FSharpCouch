module FSharpCouchIntegrationTests
    open FSharpCouch
    open System
    open NUnit.Framework
    open SpecUnit

    type TestRecord =
        { _id : string
          SomeValue : string }
    
    [<TestFixture>]      
    type FSharpCouch__when_testing_full_database_and_document_cylce () =   
        [<DefaultValue(false)>]  
        val mutable getDocumentResult : obj
        let CouchDbServer = "http://localhost:5984"
        let Database = "FSharpCouchTest"
        let FakeRecord = {_id = Guid.NewGuid().ToString(); SomeValue = "Test"}
        inherit SpecUnit.ContextSpecification()
            override x.Because () =
                FSharpCouch.CreateDatabase CouchDbServer Database |> ignore
                let myRecord = FakeRecord
                FSharpCouch.CreateDocument CouchDbServer Database FakeRecord |> ignore 
                x.getDocumentResult <- FSharpCouch.GetDocument CouchDbServer Database FakeRecord._id
                FSharpCouch.DeleteDocument CouchDbServer Database FakeRecord._id |> ignore
                FSharpCouch.DeleteDatabase CouchDbServer Database |> ignore
            [<Test>]    
            member x.should_have_a_document_with_the_expected_id () =    
                x.getDocumentResult.ShouldEqual FakeRecord._id |> ignore

