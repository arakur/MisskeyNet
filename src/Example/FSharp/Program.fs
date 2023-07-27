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

// Utilities:

let await (task: Task<'T>) =
    task |> Async.AwaitTask |> Async.RunSynchronously

// Print a note.
let printNote (name, text) =
    printfn "-- text --"
    printfn "user: %s" name
    printfn "text: %s" text

// Print a renote.
let printRenote (name, renotedName, text) =
    printfn "-- renote --"
    printfn "user: %s" renotedName
    printfn "renoted by: %s" name
    printfn "text: %s" text

// Print a note updated.
let printNoteUpdated (id, body) =
    printfn "-- note updated --"
    printfn "id: %s" id
    printfn "body: %s" body

// Print a connected message.
let printConnected id =
    printfn "-- connected --"
    printfn "id: %s" id

// Print an unknown message.
let printUnknown message =
    printfn "-- unknown --"
    printfn "%s" <| (message.ToString() |> String.take 30) + "..."

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
        // Authorize this app to Misskey instance with permission `read:account` (but not required).
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
    let! _channelConnection = streamingApi.ConnectChannelAsync(Channel.GlobalTimeline())

    printfn "channel connected"

    while true do
        try
            printfn "subscribing..."
            // Subscribe to global timeline.
            let! result = streamingApi.ReceiveAsync()

            match result with
            // A note or a renote is posted.
            | StreamMessage.Channel message ->
                let note = message.Body

                let user = note.User

                let name = defaultArg user.Name "<no name>"

                match note.Renote with
                | None -> printNote (name, defaultArg note.Text "<no text>")
                | Some renote ->
                    printRenote (name, defaultArg renote.User.Name "<no name>", defaultArg renote.Text "<no text>")

            // Connected.
            | StreamMessage.Connected message -> printConnected message.Id

            // Note updated.
            | StreamMessage.NoteUpdated message ->
                printNoteUpdated (message.Id, (message.BodyData.ToString() |> String.take 30) + "...")

            // Other messages.
            | StreamMessage.Other message -> printUnknown message

        with ex ->
            // Handle exceptions.
            printfn "ERROR OCCURS: %s" <| ex.Message

        return ()
}
|> await
