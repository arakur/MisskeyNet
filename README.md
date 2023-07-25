# Yet Another Misskey API Library for .NET (WIP)

Misskey API の .NET ライブラリの別実装です．

既存のライブラリとしては公式の実装 [Misq](https://github.com/syuilo/Misq) があります．
本ライブラリでは上記ライブラリの機能に加え，MiAuth 認証方式への対応とストリーミング API のラップを行っています．

## Installation

[NuGet](https://www.nuget.org/packages/Misskey.Net) から利用できます．

## Usage

`src/Example/FSharp/Program.fs` および `src/Example/CSharp/Program.cs` にサンプルコードがあります．

### F\#

```fsharp
open Misskey.Net.Uri
open Misskey.Net.HttpApi
open Misskey.Net.StreamingApi

open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open System.Net.WebSockets
open FSharpPlus

//

// Name of this app.
let appName = "Example App"

// Hostname of Misskey instance.
let host = "misskey.systems"

//

let client =
    ServiceCollection() // Create DI container.
        .AddHttpClient() // Add HttpClient to DI container.
        .BuildServiceProvider() // Build DI container.
        .GetService<IHttpClientFactory>() // Get IHttpClientFactory from DI container.

// Create HttpApi instance.
let httpApi = HttpApi(scheme = Https, host = host, client = client)

// Get stats of Misskey instance.
let stats =
    httpApi.RequestApiAsync [ "stats" ] |> Async.AwaitTask |> Async.RunSynchronously

printfn "stats: %s" <| stats.ToString()

let auth =
    task {
        // Authorize this app to Misskey instance with permission `read:account` (but not required).
        do! httpApi.AuthorizeAsync(name = appName, permissions = [| Permission.Read <| PermissionKind.Account() |])

        // Wait for authorization.
        return! httpApi.WaitCheckAsync()
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

if auth = false then
    failwith "authorization failed"

task {
    // Create StreamingApi instance.
    use streamingApi =
        new StreamingApi(httpApi = httpApi, webSocket = new ClientWebSocket())

    // Connect to Misskey instance.
    do! streamingApi.ConnectStreamingAsync()

    printfn "connected"

    // Connect to global timeline.
    let! _channelConnection = streamingApi.ConnectChannelAsync(Channel.GlobalTimeline())

    while true do
        // Subscribe to global timeline.
        let! result = streamingApi.ReceiveAsync()

        let content = result.["body"]

        let body = if content = null then null else content.["body"]

        if body <> null then

            let user = body.["user"]

            let nameNode = if user = null then null else user.["name"]

            let name = if nameNode = null then "<unknown>" else nameNode.ToString()

            let renote = body.["renote"]

            if renote = null then
                printfn "-- text --"

                printfn "user: %s" <| name

                let text = body.["text"]

                if text <> null then
                    printfn "text: %s" <| text.ToString()
                else
                    printfn "text: <image only>"
            else
                printfn "-- renote --"

                let text = renote.["text"]

                let renotedUser = renote.["user"]

                let renotedNameNode = if renotedUser = null then null else renotedUser.["name"]

                let renotedName =
                    if renotedNameNode = null then
                        "<unknown>"
                    else
                        renotedNameNode.ToString()

                printfn "user: %s" <| renotedName

                printfn "renoted by: %s" <| name

                if text <> null then
                    printfn "text: %s" <| text.ToString()
                else
                    printfn "text: <image only>"
        else
            printfn "unknown"

    return ()
}
|> Async.AwaitTask
|> Async.RunSynchronously
```

### C\#

```csharp
using Misskey.Net.Uri;
using Misskey.Net.HttpApi;
using Misskey.Net.StreamingApi;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;

//

// Name of this app.
var appName = "Example app";

// Hostname of Misskey instance.
var host = "misskey.systems";

//

var client =
    new ServiceCollection() // Create DI container.
        .AddHttpClient() // Add HttpClient to DI container.
        .BuildServiceProvider() // Build DI container.
        .GetService<IHttpClientFactory>(); // Get IHttpClientFactory from DI container.

// Create HttpApi instance.
var httpApi = new HttpApi(scheme: Misskey.Net.Uri.Scheme.Https, host: host, client: client);

//

// Get stats of Misskey instance
var stats = await httpApi.RequestApiAsync(new[] { "stats" });

Console.WriteLine($"stats: {stats}");

//

// Authorize this app to Misskey instance with permission `read:account` (but not required).
await httpApi.AuthorizeAsync(name: appName, permissions: new[] { Permission.NewRead(new PermissionKind.Account()) });

// Wait for authorization.
var check = await httpApi.WaitCheckAsync();

if (!check)
{
    Console.WriteLine("authorization failed");
}

Console.WriteLine("authorization completed");

//

// Create StreamingApi instance.
using var streamingApi = new StreamingApi(httpApi: httpApi, webSocket: new ClientWebSocket());

// Connect to Misskey instance.
await streamingApi.ConnectStreamingAsync();

Console.WriteLine("connected");

// Connect to global timeline.
var channelConnection = await streamingApi.ConnectChannelAsync(new Channel.GlobalTimeline());

while (true)
{
    // Subscribe to global timeline.
    var result = await streamingApi.ReceiveAsync();

    var content = result["body"];

    var body = (content == null) ? null : content["body"];

    if (body != null)
    {
        var user = body["user"];

        var nameNode = (user == null) ? "<unknown>" : user["name"];

        var name = (nameNode == null) ? "<unknown>" : nameNode.ToString();

        var renote = body["renote"];

        if (renote == null)
        {
            Console.WriteLine("-- text --");

            Console.WriteLine($"user: {name}");

            var text = body["text"];

            if (text != null)
            {
                Console.WriteLine($"text: {text}");
            }
            else
            {
                Console.WriteLine("text: <image only>");
            }
        }
        else
        {
            Console.WriteLine("-- renote --");

            var text = renote["text"];

            var renotedUser = renote["user"];

            var renotedNameNode = (renotedUser == null) ? "<unknown>" : renotedUser["name"];

            var renotedName = (renotedNameNode == null) ? "<unknown>" : renotedNameNode.ToString();

            Console.WriteLine($"user: {renotedName}");

            Console.WriteLine($"renoted by: {name}");

            if (text != null)
            {
                Console.WriteLine($"text: {text}");
            }
            else
            {
                Console.WriteLine("text: <image only>");
            }
        }
    }
    else
    {
        Console.WriteLine("unknown");
    }
}
```

## License

MIT
