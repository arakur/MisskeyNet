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

    do! httpApi.Authorize(name = "Example App", permissions = [| Permission.Write <| PermissionKind.Account() |])

    let! check = httpApi.WaitCheck()

    if not check then
        printfn "authorization failed"
    else
        printfn "authorization completed"

        //

        use websocket = new ClientWebSocket()

        let cancellationToken = CancellationToken.None

        let uri =
            Uri.Mk(Wss, host)
            |> Uri.With "streaming"
            |> Uri.WithParameter "i" httpApi.Token.Value

        printfn "connecting to %s" <| uri.ToString()

        do!
            websocket.ConnectAsync(System.Uri(uri.ToString()), cancellationToken)
            |> Async.AwaitTask

        printfn "connected"

        let buffer = Array.zeroCreate<byte> 1024

        let bufferSegment = ArraySegment(buffer)

        let connectId = Guid.NewGuid().ToString()

        // {
        //     "type": "connect",
        //     "body": {
        //         "channel": "localTimeline",
        //         "id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        //     }

        let mutable connect = JsonObject()
        connect.Add("type", "connect")

        let mutable body = JsonObject()
        body.Add("channel", "homeTimeline")
        body.Add("id", connectId)
        connect.Add("body", body)

        //

        let bytes = connect.ToString() |> Encoding.UTF8.GetBytes |> ArraySegment

        do!
            websocket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellationToken)
            |> Async.AwaitTask

        while true do
            let! result = websocket.ReceiveAsync(bufferSegment, cancellationToken) |> Async.AwaitTask

            let str = Encoding.UTF8.GetString(buffer, 0, result.Count)

            printfn "received: %s" str

        return ()
}
|> Async.RunSynchronously
