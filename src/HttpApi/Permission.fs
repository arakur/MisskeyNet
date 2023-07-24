// Generated by: HttpApi/GeneratePermission.fsx

[<RequireQualifiedAccess>]
module Misskey.Net.HttpApi.PermissionKind


/// <summary>
/// account 権限．\
/// `read:account`: アカウントの情報を見る．\
/// `write:account`: アカウントの情報を変更する．
/// </summary>
[<RequireQualifiedAccess>]
type Account() =
    interface IPermissionKind with
        override __.Name() = "account"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// blocks 権限．\
/// `read:blocks`: ブロックを見る．\
/// `write:blocks`: ブロックを操作する．
/// </summary>
type Blocks() =
    interface IPermissionKind with
        override __.Name() = "blocks"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// drive 権限．\
/// `read:drive`: ドライブを見る．\
/// `write:drive`: ドライブを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Drive() =
    interface IPermissionKind with
        override __.Name() = "drive"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// favorites 権限．\
/// `read:favorites`: お気に入りを見る．\
/// `write:favorites`: お気に入りを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Favorites() =
    interface IPermissionKind with
        override __.Name() = "favorites"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// following 権限．\
/// `read:following`: フォローの情報を見る．\
/// `write:following`: フォロー・フォロー解除する．
/// </summary>
[<RequireQualifiedAccess>]
type Following() =
    interface IPermissionKind with
        override __.Name() = "following"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// gallery-likes 権限．\
/// `read:gallery-likes`: ギャラリーのいいねを見る．\
/// `write:gallery-likes`: ギャラリーのいいねを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type GalleryLikes() =
    interface IPermissionKind with
        override __.Name() = "gallery-likes"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// messaging 権限．\
/// `read:messaging`: チャットを見る．\
/// `write:messaging`: チャットを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Messaging() =
    interface IPermissionKind with
        override __.Name() = "messaging"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// mutes 権限．\
/// `read:mutes`: ミュートを見る．\
/// `write:mutes`: ミュートを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Mutes() =
    interface IPermissionKind with
        override __.Name() = "mutes"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// notes 権限．\
/// `write:notes`: ノートを作成・削除する．
/// </summary>
[<RequireQualifiedAccess>]
type Notes() =
    interface IPermissionKind with
        override __.Name() = "notes"


    interface IWritablePermissionKind


/// <summary>
/// notifications 権限．\
/// `read:notifications`: 通知を見る．\
/// `write:notifications`: 通知を操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Notifications() =
    interface IPermissionKind with
        override __.Name() = "notifications"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// page-likes 権限．\
/// `read:page-likes`: ページのいいねを見る．\
/// `write:page-likes`: ページのいいねを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type PageLikes() =
    interface IPermissionKind with
        override __.Name() = "page-likes"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// pages 権限．\
/// `read:pages`: ページを見る．\
/// `write:pages`: ページを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Pages() =
    interface IPermissionKind with
        override __.Name() = "pages"

    interface IReadablePermissionKind
    interface IWritablePermissionKind


/// <summary>
/// reactions 権限．\
/// `write:reactions`: リアクションを操作する．
/// </summary>
[<RequireQualifiedAccess>]
type Reactions() =
    interface IPermissionKind with
        override __.Name() = "reactions"


    interface IWritablePermissionKind


/// <summary>
/// votes 権限．\
/// `write:votes`: 投票する．
/// </summary>
[<RequireQualifiedAccess>]
type Votes() =
    interface IPermissionKind with
        override __.Name() = "votes"


    interface IWritablePermissionKind
