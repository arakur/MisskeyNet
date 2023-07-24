namespace Misskey.Net.Data

[<RequireQualifiedAccess>]
module User =
    type ID = string

    type Instance =
        { name: string option
          softwareName: string option
          softwareVersion: string option
          iconUrl: string option
          faviconUrl: string option
          themeColor: string option }

    type OnlineStatus =
        | Online
        | Active
        | Offline
        | Unknown

    type Emoji = { name: string; url: string }

type UserLite =
    { id: User.ID
      username: string
      host: string option
      name: string
      onlineStatus: User.OnlineStatus
      avatarUrl: string
      avatarBlurhash: string
      emojis: User.Emoji list
      instance: User.Instance }

type User = UserLite of UserLite
