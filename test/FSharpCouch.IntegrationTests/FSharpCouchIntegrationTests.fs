module TestFSharpCouch

open FSharpCouch
open NUnit.Framework
open FsUnit

type Person = {
    FirstName : string
    LastName : string
}

let couchUrl = "http://localhost:5984"

[<Test>]
let ``When creating a document, it should succeed without an error and without creating the db first``() = 
    deleteDatabase couchUrl "people"
    createDatabase couchUrl "people"
    deleteDatabase couchUrl "people"

    let result = { FirstName = "John"; LastName = "Doe" }
                 |> createDocument couchUrl "people"
    let createdPerson = getDocument<Person> couchUrl "people" result.id 
    createdPerson.body.FirstName |> should equal "John" 
    createdPerson.body.LastName |> should equal "Doe" 
    deleteDatabase couchUrl "people"

[<Test>]
let ``When getting multiple documents, it should succeed with returning the expected results then removing everything``() = 
    deleteDatabase couchUrl "test"
    let result = 
        { FirstName = "Test"; LastName = "Test" }
        |> FSharpCouch.createDocument couchUrl "test"

    let result2 = 
        {  FirstName = "Test2"; LastName = "Test2" }
        |> FSharpCouch.createDocument couchUrl "test"

    let allDocs = FSharpCouch.getAllDocuments<Person> couchUrl "test"

    allDocs |> should haveLength 2
    let testOneDoc = Seq.head allDocs
    testOneDoc.id |> should equal result.id
    testOneDoc.body.FirstName |> should equal "Test"
    
    deleteDocument couchUrl "test" result.id result.rev

    deleteDatabase couchUrl "test"


