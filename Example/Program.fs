open FSharpPlus

open Uri
open HttpApi
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

    let httpApi = HttpApi(Https, host, client)

    //

    let! stats = httpApi.RequestApi [ "stats" ]

    printfn "stats: %s" <| stats.ToString()

    //

    do! httpApi.Authorize()

    let! check = httpApi.WaitCheck()

    if check then
        printfn "authorization completed"
    else
        printfn "authorization failed"
        return ()

    //

    let! create = httpApi.RequestApi([ "notes"; "create" ], Map.ofList [ "text", "テスト" ])

    printfn "create: %s" <| create.ToString()

    //

    use websocket = new ClientWebSocket()

    let cancellationToken = CancellationToken.None

    let uri =
        Uri.Mk(Wss, host)
        |> Uri.With "streaming"
        |> Uri.WithParameter "i" httpApi.Session.Uuid

    printfn "uri: %s" <| uri.ToString() // DEBUG

    do!
        websocket.ConnectAsync(Uri(uri.ToString()), cancellationToken)
        |> Async.AwaitTask

    printfn "connected"

    let buffer = Array.zeroCreate<byte> 1024

    let bufferSegment = ArraySegment(buffer)

    let connectId = Guid.NewGuid().ToString()

    let connect =
        JsonValue.Create
            [ "type", JsonValue.Create("connect", null)
              "body",
              JsonValue.Create
                  [ "channel", JsonValue.Create("localTimeline", null)
                    "id", JsonValue.Create(connectId, null) ] ]

    let str = connect.ToString()

    let bytes = Encoding.UTF8.GetBytes(str)

    let sendSegment = ArraySegment(bytes)

    do!
        websocket.SendAsync(sendSegment, WebSocketMessageType.Text, true, cancellationToken)
        |> Async.AwaitTask

    while true do
        let! result = websocket.ReceiveAsync(bufferSegment, cancellationToken) |> Async.AwaitTask

        let str = Encoding.UTF8.GetString(buffer, 0, result.Count)

        printfn "received: %s" str

    return ()
}
|> Async.RunSynchronously
