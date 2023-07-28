// Check ./Data.fs and generate todo list of api types in misskey-js.

open System.Net.Http
open System.Text.RegularExpressions

//

let url =
    "https://raw.githubusercontent.com/misskey-dev/misskey/7097d553e4d6f0e6273abe1710d133fe5194d628/packages/misskey-js/etc/misskey-js.api.md"

let mdPath = "./misskey-js.api.md"

let fsPath = "./Data.fs"

//

let apiMd = System.IO.File.ReadAllText(mdPath)

let pattern = @"```ts\s*([\s\S]*?)```"

let matches = Regex.Matches(apiMd, pattern)

let typesCode = matches.[0].Groups.[1].Value

//

let fsCode = System.IO.File.ReadAllText(fsPath)

//

let typeDeclPattern = @"type ([\s\S]*?) = ([\s\S]*?);\n\n"

let typeDeclMatches = Regex.Matches(typesCode, typeDeclPattern)

//

let fsTypeNamePattern0 = @"(type|and) ([\s\S]*?) ="
let fsTypeNamePattern1 = @"(type|and) ([\s\S]*?)\(json: JsonNode\) ="
// 除外するパターン "type not found"
let fsTypeExcludedNamePattern = @"type not found"

let fsTypeNameMatches =
    Regex.Matches(fsCode, fsTypeNamePattern0 + "|" + fsTypeNamePattern1)
    |> Seq.cast<Match>
    |> Seq.filter (fun m -> not <| Regex.IsMatch(m.ToString(), fsTypeExcludedNamePattern))

//

let apiTypesMap =
    typeDeclMatches
    |> Seq.cast<Match>
    |> Seq.map (fun m ->
        let name = m.Groups.[1].Value
        let value = m.Groups.[2].Value
        (name, value))

let implementedTypesSet =
    fsTypeNameMatches
    |> Seq.cast<Match>
    |> Seq.map (fun m -> m.Groups.[2].Value)
    // (.*) を削除する．
    |> Seq.map (fun s -> Regex.Replace(s, @"\(.*\)", ""))
    |> Set.ofSeq

let nonImplementedTypesMap =
    apiTypesMap
    |> Seq.filter (fun (name, _) -> not <| implementedTypesSet.Contains(name))
    |> Map.ofSeq

//

// let writeTo = fsPath

// let nonImplementedTypesCodes =
//     nonImplementedTypesMap
//     |> Map.toSeq
//     |> Seq.map (fun (name, value) ->
//         let fsMembers =
//             Regex.Matches(value, @"([a-zA-Z0-9_]+):")
//             |> Seq.cast<Match>
//             |> Seq.map (fun m -> m.Groups.[1].Value)
//             |> Seq.map (fun name -> name.[0].ToString().ToUpper() + name.Substring(1))
//             |> Seq.map (fun nameUpper -> sprintf "member this.%s: string = failwith \"TODO\"" nameUpper)
//             |> String.concat "\n    "

//         let fs =
//             $"""
// type {name}(json: JsonNode) =
//     inherit Data(json)

//     new(data: Data) = {name}(data.Json)

//     static member get (key: string) (json: JsonNode) = json |> getNode key |> {name}

//     static member tryGet (key: string) (json: JsonNode) =
//         json |> tryGetNode key |> Option.map {name}

//     // TODO
//             """
//             |> fun fs -> fs + "\n\n    " + fsMembers

//             |> fun fs -> fs + (sprintf "\ntype %s = %s;" name value |> fun s -> s.Replace("\n", "\n// "))

//         fs)

// printfn "%s" <| (nonImplementedTypesCodes |> String.concat "\n\n")

// let fsCodeAdded =
//     fsCode
//     + "\n\n// TODO\n//\n//\n\n"
//     + (nonImplementedTypesCodes |> String.concat "\n\n")

// System.IO.File.WriteAllText(writeTo, fsCodeAdded)

let nonImplementedTypesCode =
    nonImplementedTypesMap
    |> Map.toSeq
    |> Seq.map (fun (name, value) -> sprintf "type %s = %s;" name value)
    |> String.concat "\n\n"
    |> fun s -> "type TODO = any;\ntype TODO_2 = any;\ntype FIXME = any;\n\n" + s
    |> fun s -> "```ts\n" + s + "\n```"

//

let writeTo = "./misskey-js.api.todo.md"

System.IO.File.WriteAllText(writeTo, nonImplementedTypesCode)

printfn "Number of rest types = %d" nonImplementedTypesMap.Count

printfn "implemented: %s" <| (implementedTypesSet |> String.concat ", ")

printfn "not implemented: %s"
<| (nonImplementedTypesMap |> Map.keys |> String.concat ", ")
