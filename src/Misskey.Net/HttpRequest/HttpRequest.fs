namespace Misskey.Net.HttpRequest

open Misskey.Net.Uri
open System.Net.Http
open System.Text.Json.Nodes
open System.Threading.Tasks

type internal IHttpRequest =
    abstract member Request: client: IHttpClientFactory * uri: UriMk * ?content: StringContent -> Task<JsonNode>

type internal Post() =
    member __.Request(client: IHttpClientFactory, uri: UriMk, ?content: StringContent) =
        let uri = uri.ToString()

        task {
            use request = new HttpRequestMessage(HttpMethod.Post, uri.ToString())

            match content with
            | Some content -> request.Content <- content
            | None -> ()

            let! response = client.CreateClient().SendAsync request |> Async.AwaitTask
            let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
            let json = JsonValue.Parse content
            return json
        }

    interface IHttpRequest with
        member this.Request(client, uri, ?content) =
            match content with
            | Some content -> this.Request(client, uri, content)
            | None -> this.Request(client, uri)
