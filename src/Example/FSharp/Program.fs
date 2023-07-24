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
