namespace Data

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

type Note =
    { id: Note.ID
      createdAt: DateString
      text: string option
      cw: string option
      user: User
      userId: User.ID
      reply: Note option
      replyId: Note.ID
      renote: Note option
      renoteId: Note.ID
      files: DriveFile list
      fileIds: DriveFile.ID list
      visibility: Visibility
      visibleUserIds: User.ID list option
      localOnly: bool option
      myReaction: string option
      reactions: Map<string, int>
      renoteCount: int
      repliesCount: int
      poll: Note.Poll list option
      emojis: Note.Emoji list
      uri: string option
      url: string option
      isHidden: bool option }
