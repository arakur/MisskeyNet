// TODO: Generate this file from the source code of misskey-js, seriously.

module Misskey.Net.ApiTypes

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

    let getStringListList: string -> JsonNode -> string list list =
        getListWith (fun json ->
            match json with
            | :? JsonArray as array -> array |> Seq.toList |> List.map (fun json -> json.GetValue<string>())
            | _ -> failwith "invalid type: expected JsonArray")

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

type ID = string

type DateString = string

//

type Visibility =
    | Public
    | Home
    | Followers
    | Specified

//

type DriveFile(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = DriveFile(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> DriveFile

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map DriveFile

    member this.Id: ID = this.GetString "id"
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


type Instance(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Instance(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Instance

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Instance

    member this.Id: ID = this.GetString "id"
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

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module User =
    type Instance(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Instance(data.Json)

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

        new(data: Data) = Emoji(data.Json)

        static member get = getWith Emoji
        static member tryGet = tryGetWith Emoji

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

        new(data: Data) = Field(data.Json)
        static member get = getWith Field
        static member tryGet = tryGetWith Field

        member this.Name: string = this.GetString "name"
        member this.Value: string = this.GetString "value"

//

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Note =
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

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.Text: string option = this.TryGetString "text"
    member this.Cw: string option = this.TryGetString "cw"
    member this.User: UserLite = this.Json |> UserLite.get "user"
    member this.UserId: ID = this.GetString "userId"
    member this.Reply: Note option = this.Json |> Note.tryGet "reply"
    member this.ReplyId: ID = this.GetString "replyId"
    member this.Renote: Note option = this.Json |> Note.tryGet "renote"
    member this.RenoteId: ID = this.GetString "renoteId"
    member this.Files: DriveFile list = this.GetListWith DriveFile "files"

and UserLite(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = UserLite(data.Json)

    static member get = getWith UserLite

    static member tryGet = tryGetWith UserLite

    member this.Id: ID = this.GetString "id"
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
    member this.PinnedNoteIds: ID list = this.GetStringList "pinnedNoteIds"
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

    member this.Id: ID = this.AsUserLite.Id
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

    new(data: Data) = Page(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Page

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Page

    member this.Id: ID = this.GetString "id"
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

    member this.EyeCatchingImageId: ID option = this.TryGetString "eyeCatchingImageId"

    member this.EyeCatchingImage: DriveFile option =
        this.Json |> DriveFile.tryGet "eyeCatchingImage"

    member this.AttachedFiles: JsonNode list = this.GetList "attachedFiles"
    member this.LikedCount: int = this.GetInt "likedCount"
    member this.IsLiked: bool = this.GetBool "isLiked"

//

// REMARK: Untyped in the API document.
type UserGroup(json) =
    inherit Data(json)

    new(data: Data) = UserGroup(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> UserGroup

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map UserGroup

    member this.Id: ID = this.GetString "id"

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Notification =

    type Reaction(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Reaction(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Reaction

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Reaction

        member this.Reaction: string = this.GetString "reaction"
        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Reply(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Reply(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Reply

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Reply

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Renote(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Renote(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Renote

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Renote

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Quote(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Quote(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Quote

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Quote

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Mention(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Mention(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Mention

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Mention

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type PollVote(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = PollVote(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> PollVote

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"
        member this.Note: Note = this.Json |> Note.get "note"

    type Follow(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Follow(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Follow

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"

    type FollowRequestAccepted(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = FollowRequestAccepted(data.Json)

        static member get (key: string) (json: JsonNode) =
            json |> getNode key |> FollowRequestAccepted

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map FollowRequestAccepted

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"

    type ReceiveFollowRequest(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = ReceiveFollowRequest(data.Json)

        static member get (key: string) (json: JsonNode) =
            json |> getNode key |> ReceiveFollowRequest

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map ReceiveFollowRequest

        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"

    type GroupInvited(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = GroupInvited(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> GroupInvited

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map GroupInvited

        member this.Invitation: UserGroup = this.Json |> UserGroup.get "invitation"
        member this.User: User = this.Json |> User.get "user"
        member this.UserId: ID = this.GetString "userId"

    type App(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = App(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> App

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map App

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

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Notification

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Notification

    member this.Id: ID = this.GetString "id"
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

    static member get (key: string) (json: JsonNode) = json |> getNode key |> MessagingMessage

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map MessagingMessage

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.File: DriveFile option = this.Json |> DriveFile.tryGet "file"
    member this.FileId: ID option = this.TryGetString "fileId"
    member this.IsRead: bool = this.GetBool "isRead"
    member this.Reads: ID list = this.GetStringList "reads"
    member this.Text: string option = this.TryGetString "text"
    member this.User: User = this.Json |> User.get "user"
    member this.UserId: ID = this.GetString "userId"
    member this.Recipient: User option = this.Json |> User.tryGet "recipient"
    member this.RecipientId: ID option = this.TryGetString "recipientId"
    member this.Group: UserGroup option = this.Json |> UserGroup.tryGet "group"
    member this.GroupId: ID option = this.TryGetString "groupId"

//

[<RequireQualifiedAccess>]
type ChannelMessageBody =
    | Note of body: Note
    | Notification of body: Notification
    | Mention of body: Note
    | Reply of body: Note
    | Renote of body: Note
    | Follow of body: User
    | Followed of body: User
    | Unfollow of body: User
    | MessagingMessage of body: MessagingMessage
    | ReadAllNotifications
    | UnreadNotification
    | UnreadMention
    | ReadAllUnreadMentions
    | UnreadSpecifiedNote
    | ReadAllUnreadSpecifiedNotes
    | UnreadMessagingMessage
    | ReadAllMessagingMessages
    | Other of self: Data

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

    new(data: Data) = ChannelMessage(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> ChannelMessage

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map ChannelMessage

    interface IStreamMessage with
        member __.Type = StreamMessageType.Channel

    member this.Id: ID = this.GetString "id"
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

    new(data: Data) = NoteUpdatedMessage(data.Json)

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> NoteUpdatedMessage

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map NoteUpdatedMessage

    interface IStreamMessage with
        member __.Type = StreamMessageType.NoteUpdated

    member this.Id: ID = this.GetString "id"
    member this.Type: string = this.GetString "type"
    member this.BodyData: Data = Data(this.GetNode "body")

type ConnectedMessage(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = ConnectedMessage(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> ConnectedMessage


    interface IStreamMessage with
        member __.Type = StreamMessageType.Connected

    member this.Id: ID = this.GetString "id"

type OtherMessage(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = OtherMessage(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> OtherMessage

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map OtherMessage

    interface IStreamMessage with
        member this.Type = StreamMessageType.Other(this.Type)

    member this.Type: string = this.GetString "type"

[<RequireQualifiedAccess>]
type StreamMessage =
    | Channel of body: ChannelMessage
    | NoteUpdated of body: NoteUpdatedMessage
    | Connected of body: ConnectedMessage
    | Other of body: OtherMessage

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> StreamMessage.ofJson

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.bind StreamMessage.tryOfJson

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

// Miscellaneous json types.

type Acct(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Acct(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Acct

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Acct

    member this.Username: string = this.GetString "username"
    member this.Host: string option = this.TryGetString "host"

type Ad(json: JsonNode) =
    // REMARK: Untyped in the API document.
    inherit Data(json)

    new(data: Data) = Ad(data.Json)
    static member get (key: string) (json: JsonNode) = json |> getNode key |> Ad
    static member tryGet (key: string) (json: JsonNode) = json |> tryGetNode key |> Option.map Ad

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module APIError =
    type Kind =
        | Client
        | Server

        static member ofString(str: string) =
            match str with
            | "client" -> Client
            | "server" -> Server
            | _ -> failwithf "unknown error kind: %s" str

type APIError(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = APIError(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> APIError

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map APIError

    member this.Id: ID = this.GetString "id"
    member this.Code: string = this.GetString "code"
    member this.Message: string = this.GetString "message"
    member this.Kind: APIError.Kind = this.GetString "kind" |> APIError.Kind.ofString
    member this.Info: Map<string, JsonNode> = this.GetMap "info"

type Announcement(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Announcement(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Announcement

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Announcement

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.UpdatedAt: DateString option = this.TryGetString "updatedAt"
    member this.Text: string = this.GetString "text"
    member this.Title: string = this.GetString "title"
    member this.ImageUrl: string option = this.TryGetString "imageUrl"
    member this.IsRead: bool = this.GetBool "isRead"

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Antenna =
    type Src =
        | Home
        | All
        | Users
        | List
        | Group

        static member ofString(str: string) =
            match str with
            | "home" -> Home
            | "all" -> All
            | "users" -> Users
            | "list" -> List
            | "group" -> Group
            | _ -> failwithf "unknown antenna src: %s" str

type Antenna(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Antenna(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Antenna

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Antenna

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.Name: string = this.GetString "name"
    member this.Keywords: string list list = this.Json |> getStringListList "keywords"

    member this.ExcludeKeywords: string list list =
        this.Json |> getStringListList "excludeKeywords"

    member this.Src: Antenna.Src = this.GetString "src" |> Antenna.Src.ofString
    member this.UserListId: ID option = this.TryGetString "userListId"
    member this.UserGroupId: ID option = this.TryGetString "userGroupId"
    member this.Users: string list = this.GetStringList "users"
    member this.CaseSensitive: bool = this.GetBool "caseSensitive"
    member this.Notify: bool = this.GetBool "notify"
    member this.WithReplies: bool = this.GetBool "withReplies"
    member this.WithFile: bool = this.GetBool "withFile"
    member this.HasUnreadNote: bool = this.GetBool "hasUnreadNote"

// REMARK: Untyped in the API document.
type App(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = App(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> App

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map App

type AuthSession(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = AuthSession(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> AuthSession

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map AuthSession

    member this.Id: ID = this.GetString "id"
    member this.App: App = this.Json |> App.get "app"
    member this.Token: string = this.GetString "token"

type Blocking(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Blocking(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Blocking

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Blocking

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.BlockeeId: ID = this.GetString "blockeeId"
    member this.Blockee: UserDetailed = this.Json |> UserDetailed.get "blockee"

type Channel(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Channel(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Channel

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Channel

    member this.Id: ID = this.GetString "id"

// REMARK: Untyped in the API document.
type Clip(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Clip(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Clip

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Clip

type CustomEmoji(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = CustomEmoji(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> CustomEmoji

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map CustomEmoji

    member this.Id: ID = this.GetString "id"
    member this.Name: string = this.GetString "name"
    member this.Url: string = this.GetString "url"
    member this.Category: string = this.GetString "category"
    member this.Aliases: string list = this.GetStringList "aliases"


type LiteInstanceMetadata(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = LiteInstanceMetadata(data.Json)

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> LiteInstanceMetadata

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map LiteInstanceMetadata

    member this.MaintainerName: string option = this.TryGetString "maintainerName"
    member this.MaintainerEmail: string option = this.TryGetString "maintainerEmail"
    member this.Version: string = this.GetString "version"
    member this.Name: string option = this.TryGetString "name"
    member this.Uri: string = this.GetString "uri"
    member this.Description: string option = this.TryGetString "description"
    member this.Langs: string list = this.GetStringList "langs"
    member this.TosUrl: string option = this.TryGetString "tosUrl"
    member this.RepositoryUrl: string = this.GetString "repositoryUrl"
    member this.FeedbackUrl: string = this.GetString "feedbackUrl"
    member this.DisableRegistration: bool = this.GetBool "disableRegistration"
    member this.DisableLocalTimeline: bool = this.GetBool "disableLocalTimeline"
    member this.DisableGlobalTimeline: bool = this.GetBool "disableGlobalTimeline"

    member this.DriveCapacityPerLocalUserMb: int =
        this.GetInt "driveCapacityPerLocalUserMb"

    member this.DriveCapacityPerRemoteUserMb: int =
        this.GetInt "driveCapacityPerRemoteUserMb"

    member this.EmailRequiredForSignup: bool = this.GetBool "emailRequiredForSignup"
    member this.EnableHcaptcha: bool = this.GetBool "enableHcaptcha"
    member this.HcaptchaSiteKey: string option = this.TryGetString "hcaptchaSiteKey"
    member this.EnableRecaptcha: bool = this.GetBool "enableRecaptcha"
    member this.RecaptchaSiteKey: string option = this.TryGetString "recaptchaSiteKey"
    member this.EnableTurnstile: bool = this.GetBool "enableTurnstile"
    member this.TurnstileSiteKey: string option = this.TryGetString "turnstileSiteKey"
    member this.SwPublickey: string option = this.TryGetString "swPublickey"
    member this.ThemeColor: string option = this.TryGetString "themeColor"
    member this.MascotImageUrl: string option = this.TryGetString "mascotImageUrl"
    member this.BannerUrl: string option = this.TryGetString "bannerUrl"

    member this.ServerErrorImageUrl: string option =
        this.TryGetString "serverErrorImageUrl"

    member this.InfoImageUrl: string option = this.TryGetString "infoImageUrl"
    member this.NotFoundImageUrl: string option = this.TryGetString "notFoundImageUrl"
    member this.IconUrl: string option = this.TryGetString "iconUrl"
    member this.BackgroundImageUrl: string option = this.TryGetString "backgroundImageUrl"
    member this.LogoImageUrl: string option = this.TryGetString "logoImageUrl"
    member this.MaxNoteTextLength: int = this.GetInt "maxNoteTextLength"
    member this.EnableEmail: bool = this.GetBool "enableEmail"
    member this.EnableTwitterIntegration: bool = this.GetBool "enableTwitterIntegration"
    member this.EnableGithubIntegration: bool = this.GetBool "enableGithubIntegration"
    member this.EnableDiscordIntegration: bool = this.GetBool "enableDiscordIntegration"
    member this.EnableServiceWorker: bool = this.GetBool "enableServiceWorker"
    member this.Emojis: CustomEmoji list = this.GetList "emojis" |> List.map CustomEmoji
    member this.DefaultDarkTheme: string option = this.TryGetString "defaultDarkTheme"
    member this.DefaultLightTheme: string option = this.TryGetString "defaultLightTheme"
    member this.Ads: Ad list = this.GetList "ads" |> List.map Ad
    member this.TranslatorAvailable: bool = this.GetBool "translatorAvailable"
    member this.ServerRules: string list = this.GetStringList "serverRules"

type DetailedInstanceMetadata(json: JsonNode) =
    inherit LiteInstanceMetadata(json)

    new(data: Data) = DetailedInstanceMetadata(data.Json)

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> DetailedInstanceMetadata

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map DetailedInstanceMetadata

    member this.PinnedPages: string list = this.GetStringList "pinnedPages"
    member this.PinnedClipId: ID option = this.TryGetString "pinnedClipId"
    member this.CacheRemoteFiles: bool = this.GetBool "cacheRemoteFiles"
    member this.CacheRemoteSensitiveFiles: bool = this.GetBool "cacheRemoteSensitiveFiles"
    member this.RequireSetup: bool = this.GetBool "requireSetup"
    member this.ProxyAccountName: string option = this.TryGetString "proxyAccountName"
    member this.Features: Map<string, JsonNode> = this.GetMap "features"


// REMARK: Untyped in the API document.
type DriveFolder(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = DriveFolder(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> DriveFolder

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map DriveFolder

type FollowRequest(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = FollowRequest(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> FollowRequest

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map FollowRequest

    member this.Id: ID = this.GetString "id"
    member this.Follower: User = this.Json |> User.get "follower"
    member this.Followee: User = this.Json |> User.get "followee"

type Following(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Following(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Following

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Following

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.FollowerId: ID = this.GetString "followerId"
    member this.FolloweeId: ID = this.GetString "followeeId"

type FollowingFolloweePopulated(json: JsonNode) =
    inherit Following(json)

    new(data: Data) = FollowingFolloweePopulated(data.Json)

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> FollowingFolloweePopulated

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map FollowingFolloweePopulated

    member this.Followee: UserDetailed = this.Json |> UserDetailed.get "followee"

type FollowingFollowerPopulated(json: JsonNode) =
    inherit Following(json)

    new(data: Data) = FollowingFollowerPopulated(data.Json)

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> FollowingFollowerPopulated

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map FollowingFollowerPopulated

    member this.Follower: UserDetailed = this.Json |> UserDetailed.get "follower"

type GalleryPost(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = GalleryPost(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> GalleryPost

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map GalleryPost

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.UpdatedAt: DateString = this.GetString "updatedAt"
    member this.UserId: ID = this.GetString "userId"
    member this.User: User = this.Json |> User.get "user"
    member this.Title: string = this.GetString "title"
    member this.Description: string option = this.TryGetString "description"
    member this.FileIds: ID list = this.GetStringList "fileIds"
    member this.Files: DriveFile list = this.GetList "files" |> List.map DriveFile
    member this.IsSensitive: bool = this.GetBool "isSensitive"
    member this.LikedCount: int = this.GetInt "likedCount"
    member this.IsLiked: bool option = this.TryGetBool "isLiked"

type InstanceMetadata =
    | OfLiteInstanceMetadata of LiteInstanceMetadata
    | OfDetailedInstanceMetadata of DetailedInstanceMetadata

    member this.AsLiteInstanceMetadata =
        match this with
        | OfLiteInstanceMetadata metadata -> metadata
        | OfDetailedInstanceMetadata metadata -> metadata

    static member get (key: string) (json: JsonNode) =
        try
            json |> getNode key |> DetailedInstanceMetadata |> OfDetailedInstanceMetadata
        with _ ->
            json |> getNode key |> LiteInstanceMetadata |> OfLiteInstanceMetadata

    static member tryGet (key: string) (json: JsonNode) =
        try
            json
            |> tryGetNode key
            |> Option.map DetailedInstanceMetadata
            |> Option.map OfDetailedInstanceMetadata
        with _ ->
            json
            |> tryGetNode key
            |> Option.map LiteInstanceMetadata
            |> Option.map OfLiteInstanceMetadata

    member this.MaintainerName: string option = this.AsLiteInstanceMetadata.MaintainerName

    member this.MaintainerEmail: string option =
        this.AsLiteInstanceMetadata.MaintainerEmail

    member this.Version: string = this.AsLiteInstanceMetadata.Version
    member this.Name: string option = this.AsLiteInstanceMetadata.Name
    member this.Uri: string = this.AsLiteInstanceMetadata.Uri
    member this.Description: string option = this.AsLiteInstanceMetadata.Description
    member this.Langs: string list = this.AsLiteInstanceMetadata.Langs
    member this.TosUrl: string option = this.AsLiteInstanceMetadata.TosUrl
    member this.RepositoryUrl: string = this.AsLiteInstanceMetadata.RepositoryUrl
    member this.FeedbackUrl: string = this.AsLiteInstanceMetadata.FeedbackUrl
    member this.DisableRegistration: bool = this.AsLiteInstanceMetadata.DisableRegistration

    member this.DisableLocalTimeline: bool =
        this.AsLiteInstanceMetadata.DisableLocalTimeline

    member this.DisableGlobalTimeline: bool =
        this.AsLiteInstanceMetadata.DisableGlobalTimeline

    member this.DriveCapacityPerLocalUserMb: int =
        this.AsLiteInstanceMetadata.DriveCapacityPerLocalUserMb

    member this.DriveCapacityPerRemoteUserMb: int =
        this.AsLiteInstanceMetadata.DriveCapacityPerRemoteUserMb

    member this.EmailRequiredForSignup: bool =
        this.AsLiteInstanceMetadata.EmailRequiredForSignup

    member this.EnableHcaptcha: bool = this.AsLiteInstanceMetadata.EnableHcaptcha

    member this.HcaptchaSiteKey: string option =
        this.AsLiteInstanceMetadata.HcaptchaSiteKey

    member this.EnableRecaptcha: bool = this.AsLiteInstanceMetadata.EnableRecaptcha

    member this.RecaptchaSiteKey: string option =
        this.AsLiteInstanceMetadata.RecaptchaSiteKey

    member this.EnableTurnstile: bool = this.AsLiteInstanceMetadata.EnableTurnstile

    member this.TurnstileSiteKey: string option =
        this.AsLiteInstanceMetadata.TurnstileSiteKey

    member this.SwPublickey: string option = this.AsLiteInstanceMetadata.SwPublickey
    member this.ThemeColor: string option = this.AsLiteInstanceMetadata.ThemeColor
    member this.MascotImageUrl: string option = this.AsLiteInstanceMetadata.MascotImageUrl
    member this.BannerUrl: string option = this.AsLiteInstanceMetadata.BannerUrl

    member this.ServerErrorImageUrl: string option =
        this.AsLiteInstanceMetadata.ServerErrorImageUrl

    member this.InfoImageUrl: string option = this.AsLiteInstanceMetadata.InfoImageUrl

    member this.NotFoundImageUrl: string option =
        this.AsLiteInstanceMetadata.NotFoundImageUrl

    member this.IconUrl: string option = this.AsLiteInstanceMetadata.IconUrl

    member this.BackgroundImageUrl: string option =
        this.AsLiteInstanceMetadata.BackgroundImageUrl

    member this.LogoImageUrl: string option = this.AsLiteInstanceMetadata.LogoImageUrl
    member this.MaxNoteTextLength: int = this.AsLiteInstanceMetadata.MaxNoteTextLength
    member this.EnableEmail: bool = this.AsLiteInstanceMetadata.EnableEmail

    member this.EnableTwitterIntegration: bool =
        this.AsLiteInstanceMetadata.EnableTwitterIntegration

    member this.EnableGithubIntegration: bool =
        this.AsLiteInstanceMetadata.EnableGithubIntegration

    member this.EnableDiscordIntegration: bool =
        this.AsLiteInstanceMetadata.EnableDiscordIntegration

    member this.EnableServiceWorker: bool = this.AsLiteInstanceMetadata.EnableServiceWorker
    member this.Emojis: CustomEmoji list = this.AsLiteInstanceMetadata.Emojis

    member this.DefaultDarkTheme: string option =
        this.AsLiteInstanceMetadata.DefaultDarkTheme

    member this.DefaultLightTheme: string option =
        this.AsLiteInstanceMetadata.DefaultLightTheme

    member this.Ads: Ad list = this.AsLiteInstanceMetadata.Ads
    member this.TranslatorAvailable: bool = this.AsLiteInstanceMetadata.TranslatorAvailable
    member this.ServerRules: string list = this.AsLiteInstanceMetadata.ServerRules

type Invite(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Invite(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Invite

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Invite

    member this.Id: ID = this.GetString "id"
    member this.Code: string = this.GetString "code"
    member this.ExpiresAt: DateString option = this.TryGetString "expiresAt"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.CreatedBy: UserLite option = this.Json |> UserLite.tryGet "createdBy"
    member this.UsedBy: UserLite option = this.Json |> UserLite.tryGet "usedBy"
    member this.UsedAt: DateString option = this.TryGetString "usedAt"
    member this.Used: bool = this.GetBool "used"

type InviteLimit(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = InviteLimit(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> InviteLimit

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map InviteLimit

    member this.Remaining: int = this.GetInt "remaining"

type MeDetailed(json: JsonNode) =
    inherit UserDetailed(json)

    new(data: Data) = MeDetailed(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> MeDetailed

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map MeDetailed

    member this.AvatarId: ID = this.GetString "avatarId"
    member this.BannerId: ID = this.GetString "bannerId"
    member this.AutoAcceptFollowed: bool = this.GetBool "autoAcceptFollowed"
    member this.AlwaysMarkNsfw: bool = this.GetBool "alwaysMarkNsfw"
    member this.CarefulBot: bool = this.GetBool "carefulBot"

    member this.EmailNotificationTypes: string list =
        this.GetStringList "emailNotificationTypes"

    member this.HasPendingReceivedFollowRequest: bool =
        this.GetBool "hasPendingReceivedFollowRequest"

    member this.HasUnreadAnnouncement: bool = this.GetBool "hasUnreadAnnouncement"
    member this.HasUnreadAntenna: bool = this.GetBool "hasUnreadAntenna"
    member this.HasUnreadMentions: bool = this.GetBool "hasUnreadMentions"
    member this.HasUnreadMessagingMessage: bool = this.GetBool "hasUnreadMessagingMessage"
    member this.HasUnreadNotification: bool = this.GetBool "hasUnreadNotification"
    member this.HasUnreadSpecifiedNotes: bool = this.GetBool "hasUnreadSpecifiedNotes"
    member this.HideOnlineStatus: bool = this.GetBool "hideOnlineStatus"
    member this.InjectFeaturedNote: bool = this.GetBool "injectFeaturedNote"
    member this.Integrations: Map<string, JsonNode> = this.GetMap "integrations"
    member this.IsDeleted: bool = this.GetBool "isDeleted"
    member this.IsExplorable: bool = this.GetBool "isExplorable"
    member this.MutedWords: string list list = this.Json |> getStringListList "mutedWords"

    member this.MutingNotificationTypes: string list =
        this.GetStringList "mutingNotificationTypes"

    member this.NoCrawle: bool = this.GetBool "noCrawle"
    member this.ReceiveAnnouncementEmail: bool = this.GetBool "receiveAnnouncementEmail"
    member this.UsePasswordLessLogin: bool = this.GetBool "usePasswordLessLogin"
// And more untyped fields.

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MeDetailedWithSecret =
    type SecurityKey(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = SecurityKey(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> SecurityKey

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map SecurityKey

        member this.Id: ID = this.GetString "id"
        member this.Name: string = this.GetString "name"
        member this.LastUsed: DateString = this.GetString "lastUsed"

type MeDetailedWithSecret(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = MeDetailedWithSecret(data.Json)

    static member get (key: string) (json: JsonNode) =
        json |> getNode key |> MeDetailedWithSecret

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map MeDetailedWithSecret

    member this.Email: string = this.GetString "email"
    member this.EmailVerified: bool = this.GetBool "emailVerified"

    member this.SecurityKeysList: MeDetailedWithSecret.SecurityKey list =
        this.GetList "securityKeysList" |> List.map MeDetailedWithSecret.SecurityKey

type MeSignup(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = MeSignup(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> MeSignup

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map MeSignup

    member this.Token: string = this.GetString "token"

type NoteFavorite(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = NoteFavorite(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> NoteFavorite

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map NoteFavorite

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.NoteId: ID = this.GetString "noteId"
    member this.Note: Note = this.Json |> Note.get "note"

type NoteReaction(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = NoteReaction(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> NoteReaction

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map NoteReaction

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.User: UserLite = this.Json |> UserLite.get "user"
    member this.Type: string = this.GetString "type"

[<RequireQualifiedAccess>]
type OriginType =
    | Combined
    | Local
    | Remote

    static member ofString(str: string) =
        match str with
        | "combined" -> Combined
        | "local" -> Local
        | "remote" -> Remote
        | _ -> failwithf "unknown origin type: %s" str

type PageEvent(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = PageEvent(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> PageEvent

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map PageEvent

    member this.PageId: ID = this.GetString "pageId"
    member this.Event: string = this.GetString "event"
    member this.Var: JsonNode = this.GetNode "var"
    member this.UserId: ID = this.GetString "userId"
    member this.User: User = this.Json |> User.get "user"

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ServerInfo =
    type Cpu(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Cpu(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Cpu

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Cpu

        member this.Model: string = this.GetString "model"
        member this.Cores: int = this.GetInt "cores"

    type Mem(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Mem(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Mem

        static member tryGet (key: string) (json: JsonNode) =
            json |> tryGetNode key |> Option.map Mem

        member this.Total: int = this.GetInt "total"

    type Fs(json: JsonNode) =
        inherit Data(json)

        new(data: Data) = Fs(data.Json)

        static member get (key: string) (json: JsonNode) = json |> getNode key |> Fs

        static member tryGet (key: string) (json: JsonNode) = json |> tryGetNode key |> Option.map Fs

        member this.Total: int = this.GetInt "total"
        member this.Used: int = this.GetInt "used"

type ServerInfo(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = ServerInfo(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> ServerInfo

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map ServerInfo

    member this.Machine: string = this.GetString "machine"
    member this.Cpu: ServerInfo.Cpu = this.Json |> ServerInfo.Cpu.get "cpu"
    member this.Mem: ServerInfo.Mem = this.Json |> ServerInfo.Mem.get "mem"
    member this.Fs: ServerInfo.Fs = this.Json |> ServerInfo.Fs.get "fs"


type Signin(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Signin(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Signin

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Signin

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.Ip: string = this.GetString "ip"
    member this.Headers: Map<string, JsonNode> = this.GetMap "headers"
    member this.Success: bool = this.GetBool "success"

type Stats(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = Stats(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> Stats

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map Stats

    member this.NotesCount: int = this.GetInt "notesCount"
    member this.OriginalNotesCount: int = this.GetInt "originalNotesCount"
    member this.UsersCount: int = this.GetInt "usersCount"
    member this.OriginalUsersCount: int = this.GetInt "originalUsersCount"
    member this.Instances: int = this.GetInt "instances"
    member this.DriveUsageLocal: int = this.GetInt "driveUsageLocal"
    member this.DriveUsageRemote: int = this.GetInt "driveUsageRemote"

type UserList(json: JsonNode) =
    inherit Data(json)

    new(data: Data) = UserList(data.Json)

    static member get (key: string) (json: JsonNode) = json |> getNode key |> UserList

    static member tryGet (key: string) (json: JsonNode) =
        json |> tryGetNode key |> Option.map UserList

    member this.Id: ID = this.GetString "id"
    member this.CreatedAt: DateString = this.GetString "createdAt"
    member this.Name: string = this.GetString "name"
    member this.UserIds: ID list = this.GetStringList "userIds"

[<RequireQualifiedAccess>]
type UserSorting =
    | FollowerAsc
    | FollowerDesc
    | CreatedAtAsc
    | CreatedAtDesc
    | UpdatedAtAsc
    | UpdatedAtDesc

    static member ofString(str: string) =
        match str with
        | "+follower" -> FollowerAsc
        | "-follower" -> FollowerDesc
        | "+createdAt" -> CreatedAtAsc
        | "-createdAt" -> CreatedAtDesc
        | "+updatedAt" -> UpdatedAtAsc
        | "-updatedAt" -> UpdatedAtDesc
        | _ -> failwithf "unknown user sorting: %s" str
