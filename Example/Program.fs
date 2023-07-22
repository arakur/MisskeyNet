open FSharpPlus

open Uri
open HttpApi
open System.Net.Http
open Microsoft.Extensions.DependencyInjection

//

let scheme = Https

let host = "misskey.systems"

async {
    let service = ServiceCollection().AddHttpClient()

    let provider = service.BuildServiceProvider()

    let client = provider.GetService<IHttpClientFactory>()

    let httpApi = HttpApi(scheme, host, client)

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

    let id = httpApi.AuthorizedUserId.Value

    let! userInfo = httpApi.RequestApi([ "users"; "show" ], Map.ofList [ "userId", id ])

    printfn "user: %s" <| userInfo.ToString()

    return ()
}
|> Async.RunSynchronously
