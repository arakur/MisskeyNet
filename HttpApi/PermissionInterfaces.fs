namespace HttpApi

/// <summary>
/// An interface for access permissions. \
/// アクセス権限のためのインターフェース．
/// </summary>
type IPermissionKind =
    abstract member Name: unit -> string

/// <summary>
/// An interface for read permissions. \
/// 読み取り権限のためのインターフェース．
/// </summary>
type IReadablePermissionKind =
    inherit IPermissionKind

/// <summary>
/// An interface for write permissions. \
/// 書き込み権限のためのインターフェース．
/// </summary>
type IWritablePermissionKind =
    inherit IPermissionKind

/// <summary>
/// An access permission. \
/// アクセス権限．
/// </summary>
[<RequireQualifiedAccess>]
type Permission =
    /// <summary>
    /// A read permission. \
    /// 読み取り権限．
    /// </summary>
    | Read of IReadablePermissionKind
    /// <summary>
    /// A write permission. \
    /// 書き込み権限．
    /// </summary>
    | Write of IWritablePermissionKind

    /// <summary>
    /// The name of the permission. \
    /// 権限の名前．\
    /// `read:permissionName` or `write:permissionName`
    /// </summary>
    member this.Name =
        match this with
        | Read permission -> "read:" + permission.Name()
        | Write permission -> "write:" + permission.Name()
