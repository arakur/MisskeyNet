// TODO: Generate this file from the source code of misskey, seriously.

module Misskey.Net.Data

open System.Text.Json.Nodes

module internal Utils =
    let getNode (key: string) (json: JsonNode) =
        match json.[key] with
        | null -> failwithf "key %s not found" key
        | node -> node

    let tryGetNode (key: string) (json: JsonNode) =
        try
            getNode key json |> Some
        with _ ->
            None

    //

    let getWith (modifier: JsonNode -> 'T) (key: string) (json: JsonNode) = getNode key json |> modifier

    let tryGetWith (modifier: JsonNode -> 'T) (key: string) (json: JsonNode) =
        tryGetNode key json |> Option.map modifier

    //

    let private asValue (json: JsonNode) =
        match json with
        | :? JsonValue as value -> value
        | _ -> failwith "invalid type: expected JsonValue"

    let getValue: string -> JsonNode -> JsonValue = getWith asValue
    let tryGetValue: string -> JsonNode -> JsonValue option = tryGetWith asValue

    //

    let private asArray (json: JsonNode) =
        match json with
        | :? JsonArray as array -> array
        | _ -> failwith "invalid type: expected JsonArray"

    let getArray: string -> JsonNode -> JsonArray = getWith asArray
    let tryGetArray: string -> JsonNode -> JsonArray option = tryGetWith asArray

    let getList key = getArray key >> Seq.toList

    let tryGetList key =
        tryGetArray key >> Option.map Seq.toList

    let getListWith modifier key json =
        getArray key json |> Seq.map modifier |> Seq.toList

    let tryGetListWith modifier key json =
        tryGetArray key json |> Option.map (Seq.map modifier >> Seq.toList)

    //

    let getType<'T> = getWith <| fun json -> json.GetValue<'T>()
    let tryGetType<'T> = tryGetWith <| fun json -> json.GetValue<'T>()

    let getTypeList<'T> = getListWith <| fun json -> json.GetValue<'T>()
    let tryGetTypeList<'T> = tryGetListWith <| fun json -> json.GetValue<'T>()

    let getString: string -> JsonNode -> string = getType
    let tryGetString: string -> JsonNode -> string option = tryGetType
    let getInt: string -> JsonNode -> int = getType
    let tryGetInt: string -> JsonNode -> int option = tryGetType
    let getBool: string -> JsonNode -> bool = getType
    let tryGetBool: string -> JsonNode -> bool option = tryGetType

    let getStringList: string -> JsonNode -> string list = getTypeList
    let tryGetStringList: string -> JsonNode -> string list option = tryGetTypeList

    //

    let toMap (json: JsonNode) =
        match json with
        | :? JsonObject as map -> map |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq
        | _ -> failwith "invalid type: expected JsonObject"

    let getMap key json = getWith toMap key json

    let tryGetMap key json = tryGetWith toMap key json

//
//

open Utils

let private checkNull (json: JsonNode) =
    if json = null then
        failwith "given json node is null"

/// <summary>
/// A class that wraps a JSON node. \
/// JSON ノードをラップするクラス．
/// </summary>
/// <param name="json">The JSON node. JSON ノード．</param>
/// <exception cref="System.Exception">
/// Thrown when the given JSON node is null. \
/// 与えられた JSON ノードが null のときにスローされます．
/// </exception>
type Data(json: JsonNode) =
    do checkNull json

    /// <summary>
    /// Get an item of the JSON node. \
    /// JSON ノードの項目を取得します．
    /// </summary>
    /// <param name="key">The key of the item. 項目のキー．</param>
    /// <returns>The item of the JSON node. JSON ノードの項目．</returns>
    /// <remarks>
    /// This method is equivalent to `json[key]`. If the key is not found or the value is null, this method returns `null`. \
    /// このメソッドは `json[key]` と等価です．キーが見つからないか値が null のとき，このメソッドは `null` を返します．
    /// </remarks>
    member __.Item
        with get (key: string) = getNode key json

    override __.ToString() = json.ToString()

    /// <summary>
    /// The JSON node. \
    /// JSON ノード．
    /// </summary>
    member __.Json: JsonNode = json

    member __.Get<'T>(key: string) =
        getWith (fun json -> json.GetValue<'T>()) key json

    member __.TryGet<'T>(key: string) =
        tryGetWith (fun json -> json.GetValue<'T>()) key json

    /// <summary>
    /// Get an item of the JSON node. \
    /// JSON ノードの項目を取得します．
    /// </summary>
    /// <param name="key">The key of the item. 項目のキー．</param>
    /// <returns>The item of the JSON node. JSON ノードの項目．</returns>
    /// <exception cref="System.Exception">
    /// Thrown when the key is not found. キーが見つからなかったときにスローされます．
    /// </exception>
    member __.GetNode(key: string) = getNode key json

    /// <summary>
    /// If the key is found, get an item of the JSON node, otherwise return `None`. \
    /// キーが見つかった場合，JSON ノードの項目を取得します．そうでない場合，`None` を返します．
    /// </summary>
    /// <param name="key">The key of the item. 項目のキー．</param>
    /// <returns>The item of the JSON node or `None`. JSON ノードの項目または `None`．</returns>
    member __.TryGetNode(key: string) = tryGetNode key json

    // TODO: Write documentation for the rest of the members.
    member __.GetValue(key: string) = getValue key json
    member __.TryGetValue(key: string) = tryGetValue key json


    member __.GetArray(key: string) = getArray key json
    member __.TryGetArray(key: string) = tryGetArray key json

    member __.GetList(key: string) = getList key json
    member __.TryGetList(key: string) = tryGetList key json

    member __.GetString(key: string) = getString key json
    member __.TryGetString(key: string) = tryGetString key json

    member __.GetInt(key: string) = getInt key json
    member __.TryGetInt(key: string) = tryGetInt key json

    member __.GetBool(key: string) = getBool key json
    member __.TryGetBool(key: string) = tryGetBool key json

    member __.GetStringList(key: string) = getStringList key json
    member __.TryGetStringList(key: string) = tryGetStringList key json

    member __.GetMap(key: string) = getMap key json
    member __.TryGetMap(key: string) = tryGetMap key json

    member __.GetListWith<'T> (modifier: JsonNode -> 'T) (key: string) = getListWith modifier key json

    member __.TryGetListWith<'T> (modifier: JsonNode -> 'T) (key: string) = tryGetListWith modifier key json

//

type DateString = string

//

type Visibility =
    | Public
    | Home
    | Followers
    | Specified

//

[<RequireQualifiedAccess>]
module DriveFile =
    type ID = string

type DriveFile(json: JsonNode) =
    inherit Data(json)

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map DriveFile

    member this.Id: DriveFile.ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.IsSensitive: bool = this.GetBool "isSensitive"
    member this.Name: string = this.GetString "name"
    member this.ThumbnailUrl: string = this.GetString "thumbnailUrl"
    member this.Url: string = this.GetString "url"
    member this.Type: string = this.GetString "type"
    member this.Size: int = this.GetInt "size"
    member this.Md5: string = this.GetString "md5"
    member this.Blurhash: string = this.GetString "blurhash"
    member this.Comment: string option = this.TryGetString "comment"

//

[<RequireQualifiedAccess>]
module Instance =
    type ID = string

type Instance(json: JsonNode) =
    inherit Data(json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Instance

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Instance

    member this.Id: Instance.ID = this.GetString "id"
    member this.CaughtAt: DateString = this.GetString "caughtAt"
    member this.Host: string = this.GetString "host"
    member this.UsersCount: int = this.GetInt "usersCount"
    member this.NotesCount: int = this.GetInt "notesCount"
    member this.FollowingCount: int = this.GetInt "followingCount"
    member this.FollowersCount: int = this.GetInt "followersCount"
    member this.DriveUsage: int = this.GetInt "driveUsage"
    member this.DriveFiles: int = this.GetInt "driveFiles"

    member this.LatestRequestSentAt: DateString option =
        this.TryGetString "latestRequestSentAt"

    member this.LatestStatus: int option = this.TryGetInt "latestStatus"

    member this.LatestRequestReceivedAt: DateString option =
        this.TryGetString "latestRequestReceivedAt"

    member this.LastCommunicatedAt: DateString = this.GetString "lastCommunicatedAt"
    member this.IsNotResponding: bool = this.GetBool "isNotResponding"
    member this.IsSuspended: bool = this.GetBool "isSuspended"
    member this.SoftwareName: string option = this.TryGetString "softwareName"
    member this.SoftwareVersion: string option = this.TryGetString "softwareVersion"
    member this.OpenRegistrations: bool option = this.TryGetBool "openRegistrations"
    member this.Name: string option = this.TryGetString "name"
    member this.Description: string option = this.TryGetString "description"
    member this.MaintainerName: string option = this.TryGetString "maintainerName"
    member this.MaintainerEmail: string option = this.TryGetString "maintainerEmail"
    member this.IconUrl: string option = this.TryGetString "iconUrl"
    member this.FaviconUrl: string option = this.TryGetString "faviconUrl"
    member this.ThemeColor: string option = this.TryGetString "themeColor"
    member this.InfoUpdatedAt: DateString option = this.TryGetString "infoUpdatedAt"

//

[<RequireQualifiedAccess>]
module User =
    type ID = string

    type Instance(json: JsonNode) =
        inherit Data(json)

        static member get = getWith Instance
        static member tryGet = tryGetWith Instance

        member this.Name: string option = this.TryGetString "name" // REMARK A name of a user is typed as string in the API document, but it is observed that it can be null.
        member this.SoftwareName: string option = this.TryGetString "softwareName"
        member this.SoftwareVersion: string option = this.TryGetString "softwareVersion"
        member this.IconUrl: string option = this.TryGetString "iconUrl"
        member this.FaviconUrl: string option = this.TryGetString "faviconUrl"
        member this.ThemeColor: string option = this.TryGetString "themeColor"

    type OnlineStatus =
        | Online
        | Active
        | Offline
        | Unknown

        static member ofString(str: string) =
            match str with
            | "online" -> Online
            | "active" -> Active
            | "offline" -> Offline
            | "unknown" -> Unknown
            | _ -> failwithf "unknown online status: %s" str

    type Emoji(json: JsonNode) =
        inherit Data(json)

        member this.Name: string = this.GetString "name"
        member this.Url: string = this.GetString "url"

    type FfVisibility =
        | Public
        | Followers
        | Private

        static member ofString(str: string) =
            match str with
            | "public" -> Public
            | "followers" -> Followers
            | "private" -> Private
            | _ -> failwithf "unknown ff visibility: %s" str

    type Field(json: JsonNode) =
        inherit Data(json)

        member this.Name: string = this.GetString "name"
        member this.Value: string = this.GetString "value"

//

[<RequireQualifiedAccess>]
module Note =
    type ID = string

    type Choice =
        { isVoted: bool
          text: string
          votes: int }

    type Poll =
        { expiresAt: DateString option
          multiple: bool
          choices: Choice list }

    type Emoji = { name: string; url: string }

//

type Note(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Note(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Note

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Note

    member this.Id: Note.ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.Text: string option = this.TryGetString "text"
    member this.Cw: string option = this.TryGetString "cw"
    member this.User: UserLite = this.Json |> UserLite.get "user"
    member this.UserId: User.ID = this.GetString "userId"
    member this.Reply: Note option = this.Json |> Note.tryGet "reply"
    member this.ReplyId: User.ID = this.GetString "replyId"
    member this.Renote: Note option = this.Json |> Note.tryGet "renote"
    member this.RenoteId: User.ID = this.GetString "renoteId"
    member this.Files: DriveFile list = this.GetListWith DriveFile "files"

and UserLite(json: JsonNode) =
    inherit Data(json)

    static member get = getWith UserLite

    static member tryGet = tryGetWith UserLite

    member this.Id: User.ID = this.GetString "id"
    member this.Username: string = this.GetString "username"
    member this.Host: string option = this.TryGetString "host"
    member this.Name: string option = this.TryGetString "name"

    member this.OnlineStatus: User.OnlineStatus =
        this.GetString "onlineStatus" |> User.OnlineStatus.ofString

    member this.AvatarUrl: string = this.GetString "avatarUrl"
    member this.AvatarBlurhash: string = this.GetString "avatarBlurhash"

    member this.Emojis: User.Emoji list = this.GetListWith User.Emoji "emojis"

    member this.Instance: User.Instance = User.Instance(this.GetNode "instance")

and UserDetailed(json: JsonNode) =
    inherit UserLite(json)

    static member get = getWith UserDetailed
    static member tryGet = tryGetWith UserDetailed

    member this.AlsoKnownAs: string list = this.GetStringList "alsoKnownAs"
    member this.BannerBlurhash: string option = this.TryGetString "bannerBlurhash"
    member this.BannerColor: string option = this.TryGetString "bannerColor"
    member this.BannerUrl: string option = this.TryGetString "bannerUrl"
    member this.Birthday: DateString option = this.TryGetString "birthday"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.Description: string option = this.TryGetString "description"

    member this.FfVisibility: User.FfVisibility =
        this.GetString "ffVisibility" |> User.FfVisibility.ofString

    member this.Fields: User.Field list = this.GetListWith User.Field "fields"

    member this.FollowersCount: int = this.GetInt "followersCount"
    member this.FollowingCount: int = this.GetInt "followingCount"

    member this.HasPendingFollowRequestFromYou: bool =
        this.GetBool "hasPendingFollowRequestFromYou"

    member this.HasPendingFollowRequestToYou: bool =
        this.GetBool "hasPendingFollowRequestToYou"

    member this.IsAdmin: bool = this.GetBool "isAdmin"
    member this.IsBlocked: bool = this.GetBool "isBlocked"
    member this.IsBlocking: bool = this.GetBool "isBlocking"
    member this.IsBot: bool = this.GetBool "isBot"
    member this.IsCat: bool = this.GetBool "isCat"
    member this.IsFollowed: bool = this.GetBool "isFollowed"
    member this.IsFollowing: bool = this.GetBool "isFollowing"
    member this.IsLocked: bool = this.GetBool "isLocked"
    member this.IsModerator: bool = this.GetBool "isModerator"
    member this.IsMuted: bool = this.GetBool "isMuted"
    member this.IsSilenced: bool = this.GetBool "isSilenced"
    member this.IsSuspended: bool = this.GetBool "isSuspended"
    member this.Lang: string option = this.TryGetString "lang"
    member this.LastFetchedAt: DateString option = this.TryGetString "lastFetchedAt"
    member this.Location: string option = this.TryGetString "location"
    member this.MovedTo: string = this.GetString "movedTo"
    member this.NotesCount: int = this.GetInt "notesCount"
    member this.PinnedNoteIds: User.ID list = this.GetStringList "pinnedNoteIds"
    member this.PinnedNotes: Note list = this.GetList "pinnedNotes" |> List.map Note
    member this.PinnedPage: Page option = this.Json |> Page.tryGet "pinnedPage"
    member this.PinnedPageId: string option = this.TryGetString "pinnedPageId"
    member this.PublicReactions: bool = this.GetBool "publicReactions"
    member this.SecurityKeys: bool = this.GetBool "securityKeys"
    member this.TwoFactorEnabled: bool = this.GetBool "twoFactorEnabled"
    member this.UpdatedAt: DateString option = this.TryGetString "updatedAt"
    member this.Uri: string option = this.TryGetString "uri"
    member this.Url: string option = this.TryGetString "url"

and User =
    | Lite of UserLite
    | Detailed of UserDetailed

    static member fromJson(json: JsonNode) =
        try
            json |> UserDetailed |> Detailed
        with _ ->
            json |> UserLite |> Lite

    static member from(data: Data) = User.fromJson data.Json

    static member get (key: string) (json: JsonNode) =
        try
            json |> UserDetailed.get key |> Detailed
        with _ ->
            json |> UserLite.get key |> Lite

    static member tryGet (key: string) (json: JsonNode) =
        try
            Some(User.get key json)
        with _ ->
            None

    member this.AsUserLite: UserLite =
        match this with
        | Lite user -> user
        | Detailed user -> user

    member this.Id: User.ID = this.AsUserLite.Id
    member this.Username: string = this.AsUserLite.Username
    member this.Host: string option = this.AsUserLite.Host
    member this.Name: string option = this.AsUserLite.Name
    member this.OnlineStatus: User.OnlineStatus = this.AsUserLite.OnlineStatus
    member this.AvatarUrl: string = this.AsUserLite.AvatarUrl
    member this.AvatarBlurhash: string = this.AsUserLite.AvatarBlurhash
    member this.Emojis: User.Emoji list = this.AsUserLite.Emojis
    member this.Instance: User.Instance = this.AsUserLite.Instance


and Page(json: JsonNode) =
    inherit Data(json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Page

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Page

    member this.Id: string = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.UpdatedAt: DateString = this.GetString "updatedAt"
    member this.UserId: string = this.GetString "userId"
    member this.User: User = this.Json |> User.get "user"
    member this.Content: Map<string, JsonNode> list = this.GetListWith toMap "content"
    member this.Variables: Map<string, JsonNode> list = this.GetListWith toMap "variables"
    member this.Title: string = this.GetString "title"
    member this.Name: string = this.GetString "name"
    member this.Summary: string option = this.TryGetString "summary"
    member this.HideTitleWhenPinned: bool = this.GetBool "hideTitleWhenPinned"
    member this.AlignCenter: bool = this.GetBool "alignCenter"
    member this.Font: string = this.GetString "font"
    member this.Script: string = this.GetString "script"

    member this.EyeCatchingImageId: DriveFile.ID option =
        this.TryGetString "eyeCatchingImageId"

    member this.EyeCatchingImage: DriveFile option =
        this.Json |> DriveFile.tryGet "eyeCatchingImage"

    member this.AttachedFiles: JsonNode list = this.GetList "attachedFiles"
    member this.LikedCount: int = this.GetInt "likedCount"
    member this.IsLiked: bool = this.GetBool "isLiked"

//

module UserGroup =
    type ID = string

// REMARK: Untyped in the API document.
type UserGroup(json) =
    inherit Data(json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> UserGroup

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map UserGroup

    member this.Id: UserGroup.ID = this.GetString "id"

module Notification =

    type Reaction(json: JsonNode) =
        inherit Data(json)

        member this.Reaction: string = this.GetString "reaction"
        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Reply(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Renote(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Quote(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Mention(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type PollVote(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Follow(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"

    type FollowRequestAccepted(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"

    type ReceiveFollowRequest(json: JsonNode) =
        inherit Data(json)

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"

    type GroupInvited(json: JsonNode) =
        inherit Data(json)

        member this.Invitation: UserGroup = this.Json |> UserGroup.get "invitation"
        member this.User: User = this.Json |> User.get "user"
        member this.UserId: User.ID = this.GetString "userId"

    type App(json: JsonNode) =
        inherit Data(json)

        member this.Header: string option = this.TryGetString "header"
        member this.Body: string = this.GetString "body"
        member this.Icon: string option = this.TryGetString "icon"

    [<RequireQualifiedAccess>]
    type Body =
        | OfReaction of Reaction
        | OfReply of Reply
        | OfRenote of Renote
        | OfQuote of Quote
        | OfMention of Mention
        | OfPollVote of PollVote
        | OfFollow of Follow
        | OfFollowRequestAccepted of FollowRequestAccepted
        | OfReceiveFollowRequest of ReceiveFollowRequest
        | OfGroupInvited of GroupInvited
        | OfApp of App
        | Other of body: Data

type Notification(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Notification(data.Json)

    member this.Id: string = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.IsRead: bool = this.GetBool "isRead"
    member this.Type: string = this.GetString "type"

    member this.Body =
        match this.Type with
        | "reaction" -> Notification.Body.OfReaction(Notification.Reaction(this.Json))
        | "reply" -> Notification.Body.OfReply(Notification.Reply(this.Json))
        | "renote" -> Notification.Body.OfRenote(Notification.Renote(this.Json))
        | "quote" -> Notification.Body.OfQuote(Notification.Quote(this.Json))
        | "mention" -> Notification.Body.OfMention(Notification.Mention(this.Json))
        | "pollVote" -> Notification.Body.OfPollVote(Notification.PollVote(this.Json))
        | "follow" -> Notification.Body.OfFollow(Notification.Follow(this.Json))
        | "followRequestAccepted" ->
            Notification.Body.OfFollowRequestAccepted(Notification.FollowRequestAccepted(this.Json))
        | "receiveFollowRequest" ->
            Notification.Body.OfReceiveFollowRequest(Notification.ReceiveFollowRequest(this.Json))
        | "groupInvited" -> Notification.Body.OfGroupInvited(Notification.GroupInvited(this.Json))
        | "app" -> Notification.Body.OfApp(Notification.App(this.Json))
        | _ -> Notification.Body.Other(Data(this.Json))

//

type MessagingMessage(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = MessagingMessage(data.Json)

    member this.Id: string = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.File: DriveFile option = this.Json |> DriveFile.tryGet "file"
    member this.FileId: DriveFile.ID option = this.TryGetString "fileId"
    member this.IsRead: bool = this.GetBool "isRead"
    member this.Reads: User.ID list = this.GetStringList "reads"
    member this.Text: string option = this.TryGetString "text"
    member this.User: User = this.Json |> User.get "user"
    member this.UserId: User.ID = this.GetString "userId"
    member this.Recipient: User option = this.Json |> User.tryGet "recipient"
    member this.RecipientId: User.ID option = this.TryGetString "recipientId"
    member this.Group: UserGroup option = this.Json |> UserGroup.tryGet "group"
    member this.GroupId: UserGroup.ID option = this.TryGetString "groupId"

//

[<RequireQualifiedAccess>]
type ChannelMessageBody =
    | Note of Note
    | Notification of Notification
    | Mention of Note
    | Reply of Note
    | Renote of Note
    | Follow of User
    | Followed of User
    | Unfollow of User
    | MessagingMessage of MessagingMessage
    | ReadAllNotifications
    | UnreadNotification
    | UnreadMention
    | ReadAllUnreadMentions
    | UnreadSpecifiedNote
    | ReadAllUnreadSpecifiedNotes
    | UnreadMessagingMessage
    | ReadAllMessagingMessages
    | Other of body: Data

//

[<RequireQualifiedAccess>]
type StreamMessageType =
    | Channel
    | NoteUpdated
    | Connected
    | Other of ``type``: string

type IStreamMessage =
    abstract member Type: StreamMessageType

type ChannelMessage(json: JsonNode) =
    inherit Data(json)

    interface IStreamMessage with
        member __.Type = StreamMessageType.Channel

    member this.Id: string = this.GetString "id"
    member this.Type: string = this.GetString "type"

    member this.BodyData: Data = Data(this.GetNode "body")

    member this.Body: ChannelMessageBody =
        match this.Type with
        | "note" -> this.BodyData |> Note |> ChannelMessageBody.Note
        | "notification" -> this.BodyData |> Notification |> ChannelMessageBody.Notification
        | "mention" -> this.BodyData |> Note |> ChannelMessageBody.Mention
        | "reply" -> this.BodyData |> Note |> ChannelMessageBody.Reply
        | "renote" -> this.BodyData |> Note |> ChannelMessageBody.Renote
        | "follow" -> this.BodyData |> User.from |> ChannelMessageBody.Follow
        | "followed" -> this.BodyData |> User.from |> ChannelMessageBody.Followed
        | "unfollow" -> this.BodyData |> User.from |> ChannelMessageBody.Unfollow
        | "messagingMessage" -> this.BodyData |> MessagingMessage |> ChannelMessageBody.MessagingMessage
        | "readAllNotifications" -> ChannelMessageBody.ReadAllNotifications
        | "unreadNotification" -> ChannelMessageBody.UnreadNotification
        | "unreadMention" -> ChannelMessageBody.UnreadMention
        | "readAllUnreadMentions" -> ChannelMessageBody.ReadAllUnreadMentions
        | "unreadSpecifiedNote" -> ChannelMessageBody.UnreadSpecifiedNote
        | "readAllUnreadSpecifiedNotes" -> ChannelMessageBody.ReadAllUnreadSpecifiedNotes
        | "unreadMessagingMessage" -> ChannelMessageBody.UnreadMessagingMessage
        | "readAllMessagingMessages" -> ChannelMessageBody.ReadAllMessagingMessages
        | _ -> ChannelMessageBody.Other(Data(json))

type NoteUpdatedMessage(json: JsonNode) =
    inherit Data(json)

    interface IStreamMessage with
        member __.Type = StreamMessageType.NoteUpdated

    member this.Id: string = this.GetString "id"
    member this.Type: string = this.GetString "type"
    member this.BodyData: Data = Data(this.GetNode "body")

type ConnectedMessage(json: JsonNode) =
    inherit Data(json)

    interface IStreamMessage with
        member __.Type = StreamMessageType.Connected

    member this.Id: string = this.GetString "id"

type OtherMessage(json: JsonNode) =
    inherit Data(json)

    interface IStreamMessage with
        member this.Type = StreamMessageType.Other(this.Type)

    member this.Type: string = this.GetString "type"

[<RequireQualifiedAccess>]
type StreamMessage =
    | Channel of ChannelMessage
    | NoteUpdated of NoteUpdatedMessage
    | Connected of ConnectedMessage
    | Other of OtherMessage

    static member ofJson(json: JsonNode) =
        match json with
        | :? JsonObject as map ->
            let typeNode = map.["type"]

            if typeNode = null then
                failwith "type not found"

            let ``type`` = typeNode.GetValue<string>()

            match ``type`` with
            | "channel" -> Channel(ChannelMessage(map.["body"]))
            | "noteUpdated" -> NoteUpdated(NoteUpdatedMessage(map.["body"]))
            | "connected" -> Connected(ConnectedMessage(map.["body"]))
            | _ -> Other(OtherMessage(map.["body"]))
        | _ -> failwith "invalid type: expected JsonObject"

    static member tryOfJson(json: JsonNode) =
        try
            Some(StreamMessage.ofJson json)
        with _ ->
            None

    member this.Type =
        match this with
        | Channel _ -> StreamMessageType.Channel
        | NoteUpdated _ -> StreamMessageType.NoteUpdated
        | Connected _ -> StreamMessageType.Connected
        | Other other -> StreamMessageType.Other other.Type

//
