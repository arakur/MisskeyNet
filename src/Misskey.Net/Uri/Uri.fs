namespace Misskey.Net.Uri

open System.Web

/// <summary>
/// A type of a scheme of a URI.
/// It provides variants `Http` and `Https`.
/// \
/// URI のスキームの型です．
/// `Http` および `Https` のバリアントを提供します．
/// </summary>
type Scheme =
    /// <summary>
    /// The HTTP scheme. \
    /// HTTP スキーム．
    /// </summary>
    | Http
    /// <summary>
    /// The HTTPS scheme. \
    /// HTTPS スキーム．
    /// </summary>
    | Https
    /// <summary>
    /// The WS scheme. \
    /// WS スキーム．
    /// </summary>
    | Ws
    /// <summary>
    /// The WSS scheme. \
    /// WSS スキーム．
    /// </summary>
    | Wss


    override this.ToString() =
        match this with
        | Http -> "http"
        | Https -> "https"
        | Ws -> "ws"
        | Wss -> "wss"

    /// <summary>
    /// Parses a string to a URI scheme. \
    /// 文字列を URI スキームに変換します．
    /// </summary>
    /// <param name="str">A string to parse. 変換する文字列．</param>
    /// <returns>A URI scheme parsed from the string or None if the string is invalid. 文字列から変換された URI スキーム．文字列が不正な場合は None．</returns>
    static member TryFrom(str: string) =
        match str with
        | "http" -> Some Http
        | "https" -> Some Https
        | "ws" -> Some Ws
        | "wss" -> Some Wss
        | _ -> None

/// <summary>
/// URI builder. \
/// URI ビルダー．
/// </summary>
/// <remarks>
/// Directories are hold in _reverse_ order. They will be ordered in original order when stringified. \
/// ディレクトリは _逆順_ で保持されます．文字列化されるときには元の順序で並べられます．
/// </remarks>
type UriMk(scheme: Scheme, host: string, ?directories: string list, ?parameters: Map<string, string>) =
    let directories = defaultArg directories []
    let parameters = defaultArg parameters Map.empty

    member val Scheme = scheme with get, set
    member val Host = host with get, set
    member val Directories = directories with get, set
    member val Parameters = parameters with get, set


    /// <summary>
    /// Composes a URI to a string. \
    /// URI を文字列に合成します．
    /// </summary>
    ///  <remarks>
    /// Query parameters are encoded by <see cref="System.Web.HttpUtility.UrlEncode"/>. \
    /// クエリパラメータは <see cref="System.Web.HttpUtility.UrlEncode"/> によってエンコードされます．
    /// </remarks>
    /// <returns>A string composed from the URI. URI から合成された文字列．</returns>
    member this.Compose() =
        let host = this.Host |> HttpUtility.UrlEncode
        let directories = this.Directories |> List.fold (fun acc x -> $"/{x}{acc}") ""

        let query =
            if this.Parameters.IsEmpty then
                ""
            else
                this.Parameters
                |> Seq.map (fun kv -> kv.Key, HttpUtility.UrlEncode kv.Value)
                |> Seq.map (fun (k, v) -> $"{k}={v}")
                |> String.concat "&"
                |> fun x -> $"?{x}"

        $"{this.Scheme}://{host}{directories}{query}"


    override this.ToString() = this.Compose()

/// <summary>
/// A utility module for <see cref="UriMk"/>. \
/// <see cref="UriMk"/> のユーティリティモジュール．
/// </summary>
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module UriMk =
    /// <summary>
    /// Returns a new URI added a directory. \
    /// ディレクトリを追加した新しい URI を返します．
    /// </summary>
    /// <param name="dir">A directory to add. 追加するディレクトリ．</param>
    /// <param name="this">A URI to add the segment. セグメントを追加する URI．</param>
    /// <returns>A new URI added a directory. ディレクトリを追加した新しい URI．</returns>
    let withDirectory (directory: string) (this: UriMk) =
        // { this with
        //     Directories = dir :: this.Directories }
        UriMk(this.Scheme, this.Host, directories = directory :: this.Directories, parameters = this.Parameters)

    /// <summary>
    /// Returns a new URI added directories. \
    /// ディレクトリを追加した新しい URI を返します．
    /// </summary>
    /// <param name="directories">Directories to add. 追加するディレクトリ．</param>
    /// <param name="this">A URI to add the directories. ディレクトリを追加する URI．</param>
    /// <returns>A new URI added directories. ディレクトリを追加した新しい URI．</returns>
    let withDirectories (directories: string seq) (this: UriMk) =
        directories |> Seq.fold (fun acc x -> withDirectory x acc) this

    /// <summary>
    /// Returns a new URI added a parameter. \
    /// パラメータを追加した新しい URI を返します．
    /// </summary>
    /// <param name="key">A key of the parameter. パラメータのキー．</param>
    /// <param name="value">A value of the parameter. パラメータの値．</param>
    /// <param name="this">A URI to add the parameter. パラメータを追加する URI．</param>
    /// <returns>A new URI added a parameter. パラメータを追加した新しい URI．</returns>
    let withParameter (key: string) (value: string) (this: UriMk) =
        UriMk(this.Scheme, this.Host, directories = this.Directories, parameters = this.Parameters.Add(key, value))

    /// <summary>
    /// Returns a new URI added parameters. \
    /// パラメータを追加した新しい URI を返します．
    /// </summary>
    /// <param name="parameters">Parameters to add. 追加するパラメータ．</param>
    /// <param name="this">A URI to add the parameters. パラメータを追加する URI．</param>
    /// <returns>A new URI added parameters. パラメータを追加した新しい URI．</returns>
    let withParameters (parameters: (string * string) seq) (this: UriMk) =
        let newParameters =
            parameters |> Seq.fold (fun (acc: Map<_, _>) kv -> acc.Add kv) this.Parameters

        UriMk(this.Scheme, this.Host, directories = this.Directories, parameters = newParameters)
