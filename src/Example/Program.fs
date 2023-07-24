﻿open FSharpPlus

open Misskey.Net.Uri
open Misskey.Net.HttpApi
open Misskey.Net.StreamingApi

open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open System.Net.WebSockets

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

            let textNode = result.["body"].["body"].["text"]

            if textNode = null then
                let renoteNode = result.["body"].["body"].["renote"].["text"]
                printfn "renote: %s" <| renoteNode.ToString()
            else
                printfn "text: %s" <| textNode.ToString()

        return ()
}
|> Async.RunSynchronously
