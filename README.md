# Yet Another Misskey API Library for .NET (WIP)

Misskey API の .NET ライブラリの別実装です．

既存のライブラリとしては公式の実装 [Misq](https://github.com/syuilo/Misq) があります．
本ライブラリでは上記ライブラリの機能に加え，MiAuth 認証方式への対応とストリーミング API のラップを行っています．

## Usage

`src/Example/Program.fs` にサンプルコードがあります．

```fsharp
open FSharpPlus

open Uri
open HttpApi
open StreamingApi
open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open System.Net.WebSockets
open System.Threading
open System
open System.Text
open System.Text.Json.Nodes

//

let host = "misskey.systems"

async {
    let service = ServiceCollection().AddHttpClient()

    let provider = service.BuildServiceProvider()

    let client = provider.GetService<IHttpClientFactory>()

    let httpApi = HttpApi(scheme = Https, host = host, client = client)

    //

    let! stats = httpApi.RequestApiAsync [ "stats" ]

    printfn "stats: %s" <| stats.ToString()

    //

    do! httpApi.AuthorizeAsync(name = "Example App", permissions = [| Permission.Write <| PermissionKind.Account() |])

    let! check = httpApi.WaitCheckAsync()

    if not check then
        printfn "authorization failed"
    else
        printfn "authorization completed"

        //

        use streamingApi =
            new StreamingApi(httpApi = httpApi, webSocket = new ClientWebSocket())

        do! streamingApi.ConnectStreamingAsync()

        printfn "connected"

        let! _channelConnection = streamingApi.ConnectChannelAsync(Channel.GlobalTimeline())

        while true do
            let! result = streamingApi.ReceiveAsync()

            let text = result.["body"].["body"].["text"].ToString()

            let str = result.ToString()

            printfn "received: %s" str
            printfn "text: %s" text

        return ()
}
|> Async.RunSynchronously
```

## License

MIT
