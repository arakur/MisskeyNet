namespace Misskey.Net.StreamingApi

type IChannel =
    abstract member Name: string

[<RequireQualifiedAccess>]
module Channel =
    type GlobalTimeline() =
        interface IChannel with
            member __.Name = "globalTimeline"

    type HomeTimeline() =
        interface IChannel with
            member __.Name = "homeTimeline"

    type HybridTimeline() =
        interface IChannel with
            member __.Name = "hybridTimeline"

    type LocalTimeline() =
        interface IChannel with
            member __.Name = "localTimeline"

    type Main() =
        interface IChannel with
            member __.Name = "main"
