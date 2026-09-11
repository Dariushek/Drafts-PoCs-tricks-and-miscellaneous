module Snippets.FSharp.SampleTests

open NUnit.Framework
open Snippets.FSharp.Sample

[<Test>]
let ``greet returns greeting with name`` () =
    Assert.That(greet "World", Is.EqualTo "Hello, World!")
