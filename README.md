# High-Level Wrapper in .NET for Misskey API (WIP)

Misskey API の高レベルな .NET ラッパーです．

既存のライブラリとしては公式の実装 [Misq](https://github.com/syuilo/Misq) があります．
本ライブラリでは上記ライブラリの機能に加え，MiAuth 認証方式および WebSocket によるストリーミング API の利用，さらに API レスポンス JSON の静的型を提供します．
これらの機能により，Misskey クライアントや bot の開発を高レイヤのコーディングで平易に行うことができるようにすることを目指しています．

## Remark

[Misskey.Net.ApiTypes](src/Misskey.Net/ApiTypes.fs) では，[misskey-json.api.md](https://github.com/misskey-dev/misskey/blob/339086995f54197e84e9904e4778b355b02479a0/packages/misskey-js/etc/misskey-js.api.md?plain=1#L4) および [Misskey-hub](https://github.com/misskey-dev/misskey-hub/blob/main/src/docs/api/streaming/channel/main.md) に記載されている API の型情報に基づき，API レスポンスの型を定義しています．
実際のところ，これらのコードはドキュメントおよび misskey-js の実装を基に手動で作成されたものであり，テストが不十分であることに加え，misskey 本体のバージョンアップによって不正な型付けとなる可能性があります．

レスポンスに対する静的型付けが不正である場合，`ApiTypes` 下のクラスはメンバを取得しようとした段階で例外を throw します(従って，データを作成した時点では例外は発出されません)．
また，`ApiTypes` 下のクラスは内部で保持される JSON データに直接アクセスする機能を提供します：

```fsharp
let node: JsonNode = data.["property"]
```

もしライブラリの実装ミスまたはバージョンアップによる破壊的変更により不正な型付けがなされていることが確認された場合，一旦上記の代替手段を用いて JSON データを直接取得して用いてください．
そのうえで，本リポジトリの Issue にて報告をお願いします．

現在，misskey-js の実装から API の型情報を抽出しコード生成を行うスクリプトの作成を見当しています．

## Installation

[NuGet](https://www.nuget.org/packages/Misskey.Net) から利用できます．

## Usage

`src/Example/FSharp/Program.fs` および `src/Example/CSharp/Program.cs` に，ローカルタイムラインと通知を購読するサンプルコードがあります．

### F\#

```fsharp
open Misskey.Net.Uri
open Misskey.Net.HttpApi
open Misskey.Net.StreamingApi
open Misskey.Net.ApiTypes
open Misskey.Net.Permission

open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open System.Net.WebSockets
open System.Threading.Tasks

//

// Name of this app.
let appName = "Example App"

// Hostname of Misskey instance.
let Host = "misskey.systems"

//

// Utility.
let await (task: Task<'T>) =
    task |> Async.AwaitTask |> Async.RunSynchronously

//

// Main program:

// Create IHttpClientFactory instance.
let client =
    ServiceCollection() // Create DI container.
        .AddHttpClient() // Add HttpClient to DI container.
        .BuildServiceProvider() // Build DI container.
        .GetService<IHttpClientFactory>() // Get IHttpClientFactory from DI container.

// Create HttpApi instance.
let httpApi = HttpApi(scheme = Https, host = Host, client = client)

// Get stats of Misskey instance.
let stats = httpApi.RequestApiAsync [ "stats" ] |> await |> Stats

printfn "--stats --"

printfn "stats: %i" <| stats.NotesCount

printfn "users: %i" <| stats.UsersCount

printfn "instances: %i" <| stats.Instances

let auth =
    task {
        // Authorize this app to Misskey instance with permission `read:notification`.
        do!
            httpApi.AuthorizeAsync(
                name = appName,
                permissions = [| Permission.Read <| PermissionKind.Notifications() |]
            )

        // Wait for authorization.
        return! httpApi.WaitCheckAsync()
    }
    |> await

if auth = false then
    failwith "authorization failed"

printfn "authorization succeeded"

task {
    // Create StreamingApi instance.
    use streamingApi =
        new StreamingApi(httpApi = httpApi, webSocket = new ClientWebSocket())

    // Connect to Misskey instance.
    do! streamingApi.ConnectStreamingAsync()

    printfn "connected"

    // Connect to local timeline.
    let! _channelConnection = streamingApi.ConnectChannelAsync(Channel.LocalTimeline())

    printfn "connected to the local timeline channel"

    // Connect to main channel.
    let! _channelConnection = streamingApi.ConnectChannelAsync(Channel.Main())

    printfn "connected to the main channel"

    while true do
        printfn "subscribing..."
        // Subscribe to global timeline.
        let! result = streamingApi.ReceiveAsync()

        match result with
        // A note or a renote is posted.
        | StreamMessage.Channel message ->
            let body = message.Body

            match body with
            | ChannelMessageBody.Note note ->
                let user = note.User

                let name = defaultArg user.Name "<no name>"

                match note.Renote with
                | None ->
                    printfn "-- note --"
                    printfn "user: %s" name
                    printfn "text: %s" <| defaultArg note.Text "<no text>"
                | Some renote ->
                    printfn "-- renote --"
                    printfn "user: %s" name
                    printfn "renoted by: %s" <| defaultArg renote.User.Name "<no name>"
                    printfn "text: %s" <| defaultArg renote.Text "<no text>"
            | ChannelMessageBody.Notification notification ->
                match notification.Body with
                | Notification.Body.OfReaction reaction ->
                    printfn "-- reaction --"
                    printfn "user: %s" <| defaultArg reaction.User.Name "<no name>"
                    printfn "reaction: %s" <| reaction.Reaction
                    printfn "note: %s" <| defaultArg reaction.Note.Text "<no text>"
                | _ ->
                    printfn "-- other notification --"
                    printfn "%s" <| notification.Body.ToString()
            | _ -> printfn "other message %s" <| body.ToString()

        // Connected.
        | StreamMessage.Connected message ->
            printfn "-- connected --"
            printfn "id: %s" message.Id

        // Note updated.
        | StreamMessage.NoteUpdated message ->
            printfn "-- note updated --"
            printfn "id: %s" message.Id

            let body =
                message.BodyData.ToString()
                |> (fun s -> s.Substring(0, 30))
                |> (fun s -> s + "...")

            printfn "body: %s" body

        // Other messages.
        | StreamMessage.Other message -> printfn "-- other message --"

        return ()
}
|> await
```

### C\#

```csharp
using Misskey.Net;
using Misskey.Net.HttpApi;
using Misskey.Net.StreamingApi;
using Misskey.Net.Permission;
using static Misskey.Net.ApiTypes;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using Microsoft.FSharp.Core;

//

// Name of this app.
var appName = "Example app";

// Hostname of Misskey instance.
var host = "misskey.systems";

//

// Main program:

// Create IHttpClientFactory instance.
var client =
    new ServiceCollection() // Create DI container.
        .AddHttpClient() // Add HttpClient to DI container.
        .BuildServiceProvider() // Build DI container.
        .GetService<IHttpClientFactory>(); // Get IHttpClientFactory from DI container.

// Create HttpApi instance.
var httpApi = new HttpApi(scheme: Misskey.Net.Uri.Scheme.Https, host: host, client: client);

// Get stats of Misskey instance.
var stats = new Stats(await httpApi.RequestApiAsync(new[] { "stats" }));

Console.WriteLine("-- stats --");

Console.WriteLine($"notes: {stats.NotesCount}");

Console.WriteLine($"users: {stats.UsersCount}");

Console.WriteLine($"instances: {stats.Instances}");

// 

// Authorize this app to Misskey instance with permission `read:notification`.
await httpApi.AuthorizeAsync(
    name: appName,
    permissions: new[] { Permission.NewRead(new PermissionKind.Notifications()) }
);

// Wait for authorization.
var auth = await httpApi.WaitCheckAsync();

if (auth == false)
    throw new Exception("authorization failed");

Console.WriteLine("authorization succeeded");

// 

// Create StreamingApi instance.
using var streamingApi = new StreamingApi(httpApi: httpApi, webSocket: new ClientWebSocket());

// Connect to Misskey instance.
await streamingApi.ConnectStreamingAsync();

Console.WriteLine("connected");

// Connect to local timeline.
var _channelConnection = await streamingApi.ConnectChannelAsync(new Misskey.Net.StreamingApi.Channel.LocalTimeline());

Console.WriteLine("connected to the local timeline channel");

// Connect to main channel.
var _channelConnection2 = await streamingApi.ConnectChannelAsync(new Misskey.Net.StreamingApi.Channel.Main());

Console.WriteLine("connected to the main channel");

while (true)
{
    Console.WriteLine("subscribing...");
    // Subscribe to global timeline.
    var result = await streamingApi.ReceiveAsync();

    switch (result)
    {
        // A note or a renote is posted.
        case StreamMessage.Channel channelMessage:
            var body = channelMessage.body.Body;

            switch (body)
            {
                case ChannelMessageBody.Note noteMessage:
                    var note = noteMessage.body;

                    var user = note.User;

                    var name = user?.Name.Value ?? "<no name>";

                    if (FSharpOption<Note>.get_IsNone(note.Renote))
                    {
                        Console.WriteLine("-- note --");
                        Console.WriteLine($"user: {name}");
                        Console.WriteLine($"text: {note.Text.Value ?? "<no text>"}");
                    }
                    else
                    {
                        var renote = note.Renote.Value;

                        Console.WriteLine("-- renote --");
                        Console.WriteLine($"user: {name}");
                        Console.WriteLine($"renoted by: {renote.User?.Name.Value ?? "<no name>"}");
                        Console.WriteLine($"text: {renote.Text.Value ?? "<no text>"}");
                    }
                    break;
                case ChannelMessageBody.Notification notificationMessage:
                    var notification = notificationMessage.body;
                    switch (notification.Body)
                    {
                        case NotificationModule.Body.OfReaction reactionBody:
                            var reaction = reactionBody.Item;
                            Console.WriteLine("-- reaction --");
                            Console.WriteLine($"user: {reaction.User?.Name.Value ?? "<no name>"}");
                            Console.WriteLine($"reaction: {reaction.Reaction}");
                            Console.WriteLine($"note: {reaction.Note?.Text.Value ?? "<no text>"}");
                            break;
                        default:
                            Console.WriteLine("-- other notification --");
                            Console.WriteLine(notification.Body.ToString());
                            break;
                    }
                    break;
                default:
                    Console.WriteLine($"other message {body}");
                    break;
            }
            break;
        // Connected.
        case StreamMessage.Connected connectedMessage:
            var connected = connectedMessage.body;
            Console.WriteLine("-- connected --");
            Console.WriteLine($"id: {connected.Id}");
            break;
        // Note updated.
        case StreamMessage.NoteUpdated noteUpdatedMessage:
            var noteUpdated = noteUpdatedMessage.body;
            Console.WriteLine("-- note updated --");
            Console.WriteLine($"id: {noteUpdated.Id}");
            var bodyString = noteUpdated.BodyData.ToString().Substring(0, 30) + "...";
            Console.WriteLine($"body: {bodyString}");
            break;
        // Other messages.
        case StreamMessage.Other message:
            Console.WriteLine("-- other message --");
            break;
    }
}
```

## License

MIT
