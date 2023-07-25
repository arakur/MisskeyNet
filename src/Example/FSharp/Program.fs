open Misskey.Net.Uri
open Misskey.Net.HttpApi
open Misskey.Net.StreamingApi

open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open System.Net.WebSockets

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
