open Misskey.Net.Uri
open Misskey.Net.HttpApi
open Misskey.Net.StreamingApi
open Misskey.Net.Data
open Misskey.Net.Permission

open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open System.Net.WebSockets
open FSharpPlus
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
let stats = httpApi.RequestApiAsync [ "stats" ] |> await

printfn "stats: %s" <| stats.ToString()

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

    // Connect to global timeline.
    let! _channelConnection = streamingApi.ConnectChannelAsync(Channel.LocalTimeline())

    printfn "connected to the global timeline channel"

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
            let note = message.Body

            match note with
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
            | _ -> printfn "other message %s" <| note.ToString()

        // Connected.
        | StreamMessage.Connected message ->
            printfn "-- connected --"
            printfn "id: %s" message.Id

        // Note updated.
        | StreamMessage.NoteUpdated message ->
            printfn "-- note updated --"
            printfn "id: %s" message.Id
            let body = message.BodyData.ToString() |> String.take 30 |> (fun s -> s + "...")
            printfn "body: %s" body

        // Other messages.
        | StreamMessage.Other message -> printfn "-- other message --"

        return ()
}
|> await
