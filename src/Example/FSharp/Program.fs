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
                    let text = defaultArg note.Text "<image only>"
                    printfn "-- note --"
                    printfn "user: %s" name
                    printfn "text: %s" text
                | Some renote ->
                    let original = defaultArg renote.User.Name "<no name>"
                    let text = defaultArg renote.Text "<image only>"
                    printfn "-- renote --"
                    printfn "user: %s" original
                    printfn "renoted by: %s" name
                    printfn "text: %s" text
            | ChannelMessageBody.Notification notification ->
                match notification.Body with
                | Notification.Body.OfReaction reaction ->
                    let user = defaultArg reaction.User.Name "<no name>"
                    let text = defaultArg reaction.Note.Text "<image only>"
                    printfn "-- reaction --"
                    printfn "user: %s" user
                    printfn "reaction: %s" reaction.Reaction
                    printfn "note: %s" text
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
