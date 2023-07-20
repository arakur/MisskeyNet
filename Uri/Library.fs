namespace Uri

open System.Net.Http

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

    override this.ToString() =
        match this with
        | Http -> "http"
        | Https -> "https"

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
      Directories: string list }

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
          Directories = [] }

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

    override this.ToString() =
        let path = this.Directories |> List.fold (fun acc x -> $"/{x}{acc}") ""
        $"{this.Scheme.ToString()}://{this.Host}{path}"

    /// <summary>
    /// Create a post request to the URI. \
    /// URI に対する POST リクエストを作成します．
    /// </summary>
    /// <remarks>
    /// The return value must be bound by a `use` statement as the return is disposable. \
    /// 返り値は破棄可能なので，`use` 文で束縛する必要があります．
    ///　</remarks>
    /// <param name="this">A URI to create a request. リクエストを作成する URI．</param>
    /// <returns>A post request to the URI. URI に対する POST リクエスト．</returns>
    static member Post(this: Uri) =
        new HttpRequestMessage(HttpMethod.Post, this.ToString())
