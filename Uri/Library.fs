namespace Uri

open System.Net.Http
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
/// A type of a URI. \
/// URI の型です．
/// </summary>
/// <remarks>
/// Directories are hold in _reverse_ order. They will be ordered in original order when stringified. \
/// ディレクトリは _逆順_ で保持されます．文字列化されるときには元の順序で並べられます．
/// </remarks>
type Uri =
    { Scheme: Scheme
      Host: string
      Directories: string list
      Parameters: Map<string, string> }

    /// <summary>
    /// Creates a new URI without directories. \
    /// ディレクトリを持たない新しい URI を作成します．
    /// </summary>
    /// <param name="scheme">A scheme of the URI. URI のスキーム．</param>
    /// <param name="host">A host of the URI. URI のホスト．</param>
    /// <returns>A new URI without directories. ディレクトリを持たない新しい URI．</returns>
    static member Mk(scheme: Scheme, host: string) =
        { Scheme = scheme
          Host = host
          Directories = []
          Parameters = Map.empty }

    /// <summary>
    /// Returns a new URI added a directory. \
    /// ディレクトリを追加した新しい URI を返します．
    /// </summary>
    /// <param name="dir">A directory to add. 追加するディレクトリ．</param>
    /// <param name="this">A URI to add the segment. セグメントを追加する URI．</param>
    /// <returns>A new URI added a directory. ディレクトリを追加した新しい URI．</returns>
    static member With (dir: string) (this: Uri) =
        { this with
            Directories = dir :: this.Directories }

    /// <summary>
    /// Returns a new URI added directories. \
    /// ディレクトリを追加した新しい URI を返します．
    /// </summary>
    /// <param name="directories">Directories to add. 追加するディレクトリ．</param>
    /// <param name="this">A URI to add the directories. ディレクトリを追加する URI．</param>
    /// <returns>A new URI added directories. ディレクトリを追加した新しい URI．</returns>
    static member WithDirectories (directories: string seq) (this: Uri) =
        directories |> Seq.fold (fun acc x -> Uri.With x acc) this

    /// <summary>
    /// Returns a new URI added a parameter. \
    /// パラメータを追加した新しい URI を返します．
    /// </summary>
    /// <param name="key">A key of the parameter. パラメータのキー．</param>
    /// <param name="value">A value of the parameter. パラメータの値．</param>
    /// <param name="this">A URI to add the parameter. パラメータを追加する URI．</param>
    /// <returns>A new URI added a parameter. パラメータを追加した新しい URI．</returns>
    static member WithParameter (key: string) (value: string) (this: Uri) =
        { this with
            Parameters = this.Parameters.Add(key, value) }

    /// <summary>
    /// Returns a new URI added parameters. \
    /// パラメータを追加した新しい URI を返します．
    /// </summary>
    /// <param name="parameters">Parameters to add. 追加するパラメータ．</param>
    /// <param name="this">A URI to add the parameters. パラメータを追加する URI．</param>
    /// <returns>A new URI added parameters. パラメータを追加した新しい URI．</returns>
    static member WithParameters (parameters: Map<string, string>) (this: Uri) =
        let newParameters =
            parameters
            |> Map.toSeq
            |> Seq.fold (fun (acc: Map<_, _>) kv -> acc.Add kv) this.Parameters

        { this with Parameters = newParameters }


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

// TODO: remove this
// /// <summary>
// /// Create a post request to the URI. \
// /// URI に対する POST リクエストを作成します．
// /// </summary>
// /// <remarks>
// /// The return value must be bound by a `use` statement as the return is disposable. \
// /// 返り値は破棄可能なので，`use` 文で束縛する必要があります．
// ///　</remarks>
// /// <param name="this">A URI to create a request. リクエストを作成する URI．</param>
// /// <returns>A post request to the URI. URI に対する POST リクエスト．</returns>
// static member Post(this: Uri) =
//     new HttpRequestMessage(HttpMethod.Post, this.ToString())
