namespace Misskey.Net.Data

[<RequireQualifiedAccess>]
module Instance =
    type ID = string

type Instance =
    { id: Instance.ID
      caughtAt: DateString
      host: string
      usersCount: int
      notesCount: int
      followingCount: int
      followersCount: int
      driveUsage: int
      driveFiles: int
      latestRequestSentAt: DateString option
      latestStatus: int option
      latestRequestReceivedAt: DateString option
      lastCommunicatedAt: DateString
      isNotResponding: bool
      isSuspended: bool
      softwareName: string option
      softwareVersion: string option
      openRegistrations: bool option
      name: string option
      description: string option
      maintainerName: string option
      maintainerEmail: string option
      iconUrl: string option
      faviconUrl: string option
      themeColor: string option
      infoUpdatedAt: DateString option }
