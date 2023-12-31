﻿namespace Misskey.Net.HttpApi

open FSharpPlus
open System
open System.Net.Http
open System.Text

open Misskey.Net
open Misskey.Net.Uri
open Misskey.Net.Uri.UriMk
open Misskey.Net.Utils.Measure
open Misskey.Net.Permission
open Misskey.Net.ApiTypes

//

module Miauth =
    [<Literal>]
    let MIAUTH = "miauth"

open Miauth

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
    /// <summary>
    /// The session ID. \
    /// セッションID．
    /// </summary>
    member val Uuid = Guid.NewGuid().ToString() with get

    /// <summary>
    /// Check if the session ID is equal to the given UUID. \
    /// セッションIDが与えられた UUID と等しいか確認します．
    /// </summary>
    member this.CheckEquals(uuid: string) = uuid = this.Uuid


/// <summary>
/// A class that provides a HTTP API. \
/// HTTP API を提供するクラス．
/// </summary>
/// <param name="host">The host of the API. API のホスト．</param>
/// <param name="client">The HTTP client. HTTP クライアント．</param>
/// <param name="scheme">The scheme of the API. API のスキーム．</param>
type HttpApi(host: string, client: IHttpClientFactory, scheme: Scheme) =
    let mutable token = None
    let mutable authorizedUserId = None

    //

    new(host, client) = HttpApi(host, client, Https)

    //

    /// <summary>
    /// The host of the API. \
    /// API のホスト．
    /// </summary>
    member val Host = host with get

    /// <summary>
    /// The URI of the API. \
    /// API の URI． \
    /// `scheme://host.name`
    /// </summary>
    member val Uri = UriMk(scheme, host) with get

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
    member val Client = client with get

    //

    /// <summary>
    /// The URI of the API. \
    /// API の URI． \
    /// `https://host.name/api`
    /// </summary>
    member this.ApiUri = this.Uri |> withDirectory "api"

    /// <summary>
    /// The URI of the API. \
    /// API の URI． \
    /// `https://host.name/api`
    /// </summary>
    member this.StreamingUri = this.Uri |> withDirectory "api"

    /// <summary>
    /// The URI of the authorization page. \
    /// 認証ページの URI． \
    /// `https://host.name/miauth/sessionID`
    /// </summary>
    member this.MiAuthUri =
        this.Uri |> withDirectory MIAUTH |> withDirectory this.Session.Uuid

    // TODO: Add an option to specify permissions.

    /// <summary>
    /// Open the authorization page in the default browser. \
    /// デフォルトブラウザで認証ページを開きます．
    /// </summary>
    member this.AuthorizeAsync(?name: string, ?icon: string, ?callback: string, ?permissions: Permission seq) =
        // Combine a command and arguments depending on the platform.
        // TODO: Check if the command works on platforms other than Windows.

        let os = Environment.OSVersion.Platform

        let nameQuery = name |>> (fun x -> "name", x)

        let iconQuery = icon |>> (fun x -> "icon", x)

        let callbackQuery = callback |>> (fun x -> "callback", x)

        let permissionsQuery =
            permissions
            |>> (Seq.map (fun (permission: Permission) -> permission.Name)
                 >> String.concat ","
                 >> fun x -> "permission", x)

        let queries =
            [| nameQuery; iconQuery; callbackQuery; permissionsQuery |] |> Seq.choose id

        let miAuthUri =
            this.MiAuthUri |> withParameters queries |> (fun uri -> uri.ToString())

        let fileName, arguments =
            match os with
            | PlatformID.Win32NT
            | PlatformID.Win32S
            | PlatformID.Win32Windows
            | PlatformID.WinCE -> "powershell.exe", $"-Command Start-Process '{miAuthUri}'"
            | PlatformID.Unix
            | PlatformID.MacOSX -> "sh", $"-c 'open {miAuthUri}'"
            | _ -> failwith "unsupported platform"

        task {
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
    member this.CheckAsync() =
        let uri =
            this.ApiUri
            |> withDirectory "miauth"
            |> withDirectory this.Session.Uuid
            |> withDirectory "check"

        task {
            // post request
            use request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())

            let! json = HttpRequest.Post().Request(this.Client, uri)

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
    member this.WaitCheckAsync(?span: float<millisecond>, ?timeout: float<millisecond>, ?silent: bool) =
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
            task { return true }
        else if timeout <= 0.<millisecond> then
            printUnlessSilent "timeout"
            task { return false }
        else
            printUnlessSilent "checking..."

            task {
                let! check = this.CheckAsync()

                if check then
                    printUnlessSilent "authorized"
                    return true
                else
                    printUnlessSilent "not yet authorized"
                    do! Async.Sleep(int span)
                    return! this.WaitCheckAsync(span, timeout - span, silent)
            }

    /// <summary>
    /// Request the API. \
    /// API をリクエストします．
    /// </summary>
    /// <param name="endPointName">The name of the endpoint. エンドポイントの名前．</param>
    /// <param name="payload">The payload. ペイロード．default: `[]`</param>
    /// <returns>
    /// The response. \
    /// レスポンス．
    /// </returns>
    member this.RequestApiAsync(endPointNameSeq: string seq, ?payload: (string * string) seq) =
        let payload = defaultArg payload []

        // Add the access token to the payload if it exists.
        let payload =
            match this.Token with
            | Some token -> payload |> Seq.append [ "i", token ]
            | None -> payload

        // Make payload into JSON.
        let json =
            payload |>> (fun (k, v) -> $"\"{k}\":\"{v}\"")
            |> String.concat ","
            |> fun x -> $"{{{x}}}"

        let uri = this.ApiUri |> withDirectories endPointNameSeq

        task {
            use stringContent = new StringContent(json, Encoding.UTF8, "application/json")

            let! response = HttpRequest.Post().Request(this.Client, uri, stringContent)

            return response |> Data
        }

// TODO: Generate functions for each endpoint.
