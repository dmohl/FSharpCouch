FSharpCouch
=======

**FSharpCouch** is a fairly simple wrapper around many of the most common aspects of the CouchDB API. It provides an API that works well with F# records, function composition, etc.

Syntax
=======

Here are a few examples to get you started with using FSharpCouch:

	type Person = {
		FirstName : string
		LastName : string
	}

	let couchUrl = "http://localhost:5984"
    let databaseName = "people"

Create a document (if the database doesn't exist, it will be created):
    
    let result = { FirstName = "John"; LastName = "Doe" }
                 |> createDocument couchUrl databaseName

Get a document:

    let createdPerson = getDocument<Person> couchUrl databaseName result.id 

Get all documents:

	let allDocs = FSharpCouch.getAllDocuments<Person> couchUrl databaseName
	
Delete a document:

    deleteDocument couchUrl documentName result.id result.rev	

Create a database: 

    createDatabase couchUrl databaseName
	
Delete a database:

    deleteDatabase couchUrl databaseName

How To Get It
=======

FSharpCouch is available on NuGet Gallery as id FSharpCouch.

Releases
=======
* 0.2.0.0 - Provides several improvements primarily related to document retrieval.
* 0.1.0.0 - Was created for a blog post a few years ago.

MIT License
=======

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.