# Yet Another Misskey API Library for .NET (WIP)

Misskey API の .NET ライブラリの別実装です．

既存のライブラリとしては公式の実装 [Misq](https://github.com/syuilo/Misq) があります．
本ライブラリでは上記ライブラリの機能に加え，MiAuth 認証方式への対応とストリーミング API のラップを行っています．

## Installation

[NuGet](https://www.nuget.org/packages/Misskey.Net) から利用できます．

## Usage

`src/Example/Program.fs` にサンプルコードがあります．

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

task {
    let client =
        ServiceCollection() // Create DI container.
            .AddHttpClient() // Add HttpClient to DI container.
            .BuildServiceProvider() // Build DI container.
            .GetService<IHttpClientFactory>() // Get IHttpClientFactory from DI container.

    // Create HttpApi instance.
    let httpApi = HttpApi(scheme = Https, host = host, client = client)

    //

    // Get stats of Misskey instance.
    let! stats = httpApi.RequestApiAsync [ "stats" ]

    printfn "stats: %s" <| stats.ToString()

    //

    // Authorize this app to Misskey instance with permission `read:account` (but not required).
    do! httpApi.AuthorizeAsync(name = appName, permissions = [| Permission.Read <| PermissionKind.Account() |])

    // Wait for authorization.
    let! check = httpApi.WaitCheckAsync()

    if not check then
        printfn "authorization failed"
    else
        printfn "authorization completed"

        //

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

            let body = if content.["body"] = null then null else content.["body"]

            if body <> null then
                let text = result.["body"].["body"].["text"]

                if text = null then
                    let renoteNode = body.["renote"]
                    let renote = if renoteNode = null then null else renoteNode.["text"]
                    printfn "renote: %s" <| renote.ToString()
                else
                    printfn "text: %s" <| text.ToString()

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
else
{
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

        var body = content == null ? null : content["body"];

        if (body != null)
        {
            var textNode = body["text"];
            if (textNode == null)
            {
                var renoteNode = body == null ? null : body["renote"];
                var renoteText = renoteNode == null ? null : renoteNode["text"];
                Console.WriteLine($"renote: {renoteText}");
            }
            else
            {
                Console.WriteLine($"text: {textNode}");
            }
        }
    }
}
```

## License

MIT
