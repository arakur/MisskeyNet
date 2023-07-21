﻿namespace HttpApi

open FSharpPlus
open System
open System.Net.Http
open System.Text
open System.Text.Json.Nodes

open Utils.Measure
open Uri

//

/// <summary>
/// A type synonym for access tokens. \
/// アクセストークンの型シノニム．
/// </summary>
type Token = string


/// <summary>
/// A class that generates and holds a session ID. \
/// セッションIDを生成し保持するクラス．
/// </summary>
/// <remarks>
/// The session ID is UUID generated by <see cref="System.Guid.NewGuid"/>. \
/// セッションIDは <see cref="System.Guid.NewGuid"/> によって生成された UUID です．
/// </remarks>
type Session() =
    member val Uuid = Guid.NewGuid().ToString() with get


/// <summary>
/// A class that provides a HTTP API. \
/// HTTP API を提供するクラス．
/// </summary>
/// <param name="scheme">The scheme of the API. API のスキーム．</param>
/// <param name="host">The host of the API. API のホスト．</param>
type HttpApi(scheme: Scheme, host: string) =
    let mutable token = None
    let mutable authorizedUserId = None

    //

    /// <summary>
    /// The URI of the API. \
    /// API の URI． \
    /// `scheme://host.name`
    /// </summary>
    member val Uri = Uri.Mk(scheme, host) with get

    /// <summary>
    /// The access token. \
    /// アクセストークン．
    /// </summary>
    member __.Token = token

    /// <summary>
    /// The ID of the authorized user. \
    /// 認証されたユーザーのID．
    /// </summary>
    member __.AuthorizedUserId = authorizedUserId

    /// <summary>
    /// The session ID. \
    /// セッションID．
    /// </summary>
    member val Session = Session() with get

    /// <summary>
    /// The HTTP client. \
    /// HTTP クライアント．
    /// </summary>
    member val Client = new HttpClient() with get

    interface System.IDisposable with
        member this.Dispose() = this.Client.Dispose()

    //

    /// <summary>
    /// The URI of the API. \
    /// API の URI． \
    /// `https://host.name/api`
    /// </summary>
    member this.ApiUri = this.Uri |> Uri.With "api"

    /// <summary>
    /// The URI of the authorization page. \
    /// 認証ページの URI． \
    /// `https://host.name/miauth/sessionID`
    /// </summary>
    member this.MiAuthUri = this.Uri |> Uri.With "miauth" |> Uri.With this.Session.Uuid

    // TODO: Add an option to specify permissions.

    /// <summary>
    /// Open the authorization page in the default browser. \
    /// デフォルトブラウザで認証ページを開きます．
    /// </summary>
    member this.Authorize() =
        // Combine a command and arguments depending on the platform.
        // TODO: Check if the command works on platforms other than Windows.

        let os = Environment.OSVersion.Platform

        let fileName, arguments =
            match os with
            | PlatformID.Win32NT
            | PlatformID.Win32S
            | PlatformID.Win32Windows
            | PlatformID.WinCE -> "powershell.exe", $"-Command Start-Process '{this.MiAuthUri.ToString()}'"
            | PlatformID.Unix
            | PlatformID.MacOSX -> "sh", $"-c 'open {this.MiAuthUri.ToString()}'"
            | _ -> failwith "unsupported platform"

        async {
            let startInfo =
                Diagnostics.ProcessStartInfo(
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                )

            use process_ = new Diagnostics.Process(StartInfo = startInfo)

            process_.Start() |> ignore

            process_.WaitForExit()
        }

    /// <summary>
    /// Check if the authorization is completed.
    /// If the authorization is completed, the access token is set. \
    /// 認証が完了しているか確認します．
    /// 認証が完了している場合，アクセストークンが設定されます．
    /// </summary>
    /// <returns>
    /// </returns>
    member this.Check() =
        let uri =
            this.ApiUri
            |> Uri.With "miauth"
            |> Uri.With this.Session.Uuid
            |> Uri.With "check"

        async {
            // post request
            use request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())

            let! response = this.Client.SendAsync request |> Async.AwaitTask

            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            let json = JsonValue.Parse content

            let ok = json.["ok"].ToString()

            if ok = "true" then
                token <- Some(json.["token"].ToString())
                let user = json.["user"]
                let id = user.["id"].ToString()
                authorizedUserId <- Some(id)

                return true
            else
                return false
        }

    /// <summary>
    /// Wait until the authorization is completed. \
    /// 認証が完了するまで待機します．
    /// </summary>
    /// <param name="span">The time span between checks. チェック間の時間間隔．default: 1000 ms</param>
    /// <param name="timeout">The timeout. タイムアウト．default: 10000 ms</param>
    /// <param name="silent">If `false`, print logs. `false` の場合，ログを出力します．default: `true`</param>
    /// <returns>
    /// `true` if the authorization is completed. \
    /// 認証が完了している場合 `true` を返します．
    /// </returns>
    member this.WaitCheck(?span: float<millisecond>, ?timeout: float<millisecond>, ?silent: bool) =
        // Set default values.
        let span = defaultArg span 1000.<millisecond>
        let timeout = defaultArg timeout 10000.<millisecond>
        let silent = defaultArg silent false

        // Print a message unless `silent` is `true`.
        let printUnlessSilent x =
            if not silent then
                printfn x

        if this.Token.IsSome then
            printUnlessSilent "already authorized"
            async { return true }
        else if timeout <= 0.<millisecond> then
            printUnlessSilent "timeout"
            async { return false }
        else
            printUnlessSilent "checking..."

            async {
                let! check = this.Check()

                if check then
                    printUnlessSilent "authorized"
                    return true
                else
                    printUnlessSilent "not yet authorized"
                    do! Async.Sleep(int span)
                    return! this.WaitCheck(span, timeout - span, silent)
            }

    /// <summary>
    /// Request the API. \
    /// API をリクエストします．
    /// </summary>
    /// <param name="endPointName">The name of the endpoint. エンドポイントの名前．</param>
    /// <param name="payload">The payload. ペイロード．default: `Map.empty`</param>
    /// <returns>
    /// The response. \
    /// レスポンス．
    /// </returns>
    member this.RequestApi(endPointName: string, ?payload: Map<string, string>) =
        // The default payload is an empty map.
        let payload = defaultArg payload Map.empty

        // Add the access token to the payload if it exists.
        let payload =
            match this.Token with
            | Some token -> payload |> Map.add "i" token
            | None -> payload

        // Make payload into JSON.
        let json =
            payload
            |> Map.toSeq
            |> map (fun (k, v) -> $"\"{k}\":\"{v}\"")
            |> String.concat ","
            |> fun x -> $"{{{x}}}"

        let uri = this.ApiUri |> Uri.With endPointName

        async {
            use stringContent = new StringContent(json, Encoding.UTF8, "application/json")

            use request = uri |> Uri.Post

            request.Content <- stringContent

            let! message = this.Client.SendAsync request |> Async.AwaitTask

            let! content = message.Content.ReadAsStringAsync() |> Async.AwaitTask

            let json = JsonValue.Parse content

            return json
        }

// TODO: Generate functions for each endpoint.
