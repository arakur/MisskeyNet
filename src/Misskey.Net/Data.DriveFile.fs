namespace Misskey.Net.Data

open System.Text.Json.Nodes

open Utils

[<RequireQualifiedAccess>]
module DriveFile =
    type ID = string

type DriveFile(json: JsonNode) =
    do
        if json = null then
            failwith "given json node is null"

    static member tryGet (json: JsonNode) (key: string) =
        tryGetNode key json |> Option.map DriveFile

    member __.Item
        with get (key: string) = getNode key json

    member val Id: DriveFile.ID = json |> getString "id" with get
    member val CreatedAt: DateString = json |> getString "createdAt" with get
    member val IsSensitive: bool = json |> getBool "isSensitive" with get
    member val Name: string = json |> getString "name" with get
    member val ThumbnailUrl: string = json |> getString "thumbnailUrl" with get
    member val Url: string = json |> getString "url" with get
    member val Type: string = json |> getString "type" with get
    member val Size: int = json |> getInt "size" with get
    member val Md5: string = json |> getString "md5" with get
    member val Blurhash: string = json |> getString "blurhash" with get
    member val Comment: string option = json |> tryGetString "comment" with get
