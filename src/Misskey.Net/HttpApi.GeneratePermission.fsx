// Generate Permission.fs.
// TODO: Use Fantomas.Core (https://fsprojects.github.io/fantomas/docs/end-users/GeneratingCode.html#Generating-source-code-from-scratch).

let url =
    "https://raw.githubusercontent.com/misskey-dev/misskey-hub/main/src/docs/api/permission.md"

//

open System.Net.Http

//

let thisUrl = "GeneratePermission.fsx"

// Make a kebab-case string to a PascalCase string.
let toPascalCase (s: string) =
    s.Split("-")
    |> Array.map (fun s -> s.[0].ToString().ToUpper() + s.[1..])
    |> String.concat ""

type Permission =
    { read: string option
      write: string option }

    static member None = { read = None; write = None }

    static member Add kind description this =
        match kind with
        | "read" -> { this with read = Some description }
        | "write" -> { this with write = Some description }
        | _ -> failwith "Invalid permission kind"

let buildType (name: string) (permission: Permission) =
    let read =
        permission.read
        |> Option.map (fun description -> $"/// `read:{name}`: {description}．")

    let write =
        permission.write
        |> Option.map (fun description -> $"/// `write:{name}`: {description}．")

    let readWrite = [ read; write ] |> List.choose id |> String.concat "\\\n"

    let description =
        $"""
/// <summary>
/// {name} 権限．\
{readWrite}
/// </summary>
        """
        |> (fun s -> s.Trim())

    let namePascal = toPascalCase name

    $"""
{description}
type {namePascal}() =
    interface IPermissionKind with
        override __.Name() = "{name}"
    
    {if read.IsSome then
         "interface IReadablePermissionKind"
     else
         ""}
    {if write.IsSome then
         "interface IWritablePermissionKind"
     else
         ""}
    """
    |> (fun s -> s.Replace("\n", "\n    "))

//

let client = new HttpClient()

let permissionMd = client.GetStringAsync(url).Result

let parts = permissionMd.Split("#")

assert (parts.Length >= 2)

let _header = parts.[0]

let rest = parts.[1..]

let permissionPart =
    rest
    |> Array.map (fun part -> part.Split("\n"))
    |> Array.find (fun lines ->
        let sectionName = lines.[0]
        sectionName.Contains("権限の一覧"))

let permissionTable =
    permissionPart.[1..]
    |> Array.filter (fun row -> row.StartsWith("|")) // Choose only rows whi
    |> Array.map (fun row -> row.Split("|"))
    |> Array.map (Array.map (fun cell -> cell.Trim()))

assert (permissionTable.Length >= 3)

let permissionHeader = permissionTable.[0]
let _permissionSeparator = permissionTable.[1]
let permissionRows = permissionTable.[2..]

// REMARK: `Permisson` is typo in the document. It will be removed after it is fixed.
let permissionColumn =
    permissionHeader
    |> Array.findIndex (fun column -> column.Contains("Permission") || column.Contains("Permisson"))

let descriptionColumn =
    permissionHeader
    |> Array.findIndex (fun column -> column.Contains("Description"))

let permissionArray =
    permissionRows
    |> Array.map (fun row ->
        let permissionQuoted = row.[permissionColumn]
        let description = row.[descriptionColumn]

        let permission = permissionQuoted.[1 .. permissionQuoted.Length - 2]

        let kind, name = permission.Split ":" |> fun parts -> parts.[0], parts.[1]

        kind, name, description)

let permissionMap: Map<string, Permission> =
    permissionArray
    |> Array.fold
        (fun map (kind, name, description) ->
            let permission =
                map.TryFind name
                |> Option.defaultValue Permission.None
                |> Permission.Add kind description

            map.Add(name, permission))
        Map.empty

let code =
    permissionMap
    |> Map.toSeq
    |> Seq.map (fun (name, permission) -> buildType name permission)
    |> String.concat "\n"
    |> fun s ->
        $"""// Generated by: {thisUrl}

namespace Misskey.Net.Permission

[<RequireQualifiedAccess>]
module PermissionKind =

{s}
"""

let writeTo = "../Permission.fs"

System.IO.File.WriteAllText(writeTo, code)
