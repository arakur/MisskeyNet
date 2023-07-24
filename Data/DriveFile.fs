namespace Data

[<RequireQualifiedAccess>]
module DriveFile =
    type ID = string

type DriveFile =
    { id: DriveFile.ID
      createdAt: DateString
      isSensitive: bool
      name: string
      thumbnailUrl: string
      url: string
      ``type``: string
      size: int
      md5: string
      blurhash: string
      comment: string option
      properties: Map<string, string> }
