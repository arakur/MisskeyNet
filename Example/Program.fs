open FSharpPlus

open Uri
open HttpApi

//

let scheme = Https

let host = "misskey.systems"

async {
    use httpApi = new HttpApi(scheme, host)

    //

    let! stats = httpApi.RequestApi "stats"

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

    let! userInfo = httpApi.RequestApi("users/show", Map.ofList [ "userId", id ])

    printfn "user: %s" <| userInfo.ToString()

    return ()
}
|> Async.RunSynchronously
