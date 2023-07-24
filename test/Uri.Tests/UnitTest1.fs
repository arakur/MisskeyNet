module Misskey.Net.Uri.Tests

open Misskey.Net.Uri

open NUnit.Framework
open Microsoft.FSharp.Reflection

[<TestFixture>]
type TestClass() =

    [<Test>]
    member __.TestSchemeToString() =

        let schemes =
            FSharpType.GetUnionCases(typeof<Scheme>)
            |> Seq.map (fun x -> FSharpValue.MakeUnion(x, Array.zeroCreate (x.GetFields().Length)) :?> Scheme)

        for scheme in schemes do
            let str = scheme.ToString()
            let scheme' = Scheme.TryFrom(str)

            Assert.AreEqual(Some scheme, scheme')

    [<Test>]
    member __.TestUriToString() =
        let scheme = Https
        let host = "misskey.ふーばー"
        let directories = [ "foo"; "bar" ]
        let parameters = [| "foo", "ばー"; "url", "https://misskey.ふーばー/callback" |]

        let uri =
            Uri.Mk(scheme, host)
            |> Uri.withDirectories directories
            |> Uri.withParameters parameters

        let str = uri.ToString()

        let uri' =
            "https://misskey.%e3%81%b5%e3%83%bc%e3%81%b0%e3%83%bc/foo/bar?foo=%e3%81%b0%e3%83%bc&url=https%3a%2f%2fmisskey.%e3%81%b5%e3%83%bc%e3%81%b0%e3%83%bc%2fcallback"

        Assert.AreEqual(uri', str)
