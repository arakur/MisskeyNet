namespace Misskey.Net.StreamingApi

// TODO: Use a kind of ClientWebSocketFactory instead of ClientWebSocket.
// TODO: Return messages with deserializing.

open Misskey.Net.Uri
open Misskey.Net.HttpApi

open System.Net.WebSockets
open System.Threading
open System
open System.Text
open System.Text.Json.Nodes

//

type ChannelConnection(channel: IChannel) =
    member val Channel = channel with get
    member val internal Uuid = Guid.NewGuid().ToString() with get

type NoteSubscription(noteId: string) =
    member val NoteId = noteId with get

//

type StreamingApi(httpApi: HttpApi, webSocket: ClientWebSocket, bufferSize: int) =
    static member private DEFAULT_BUFFER_SIZE = 65536

    new(httpApi: HttpApi, webSocket: ClientWebSocket) =
        new StreamingApi(httpApi, webSocket, StreamingApi.DEFAULT_BUFFER_SIZE)

    member val HttpApi = httpApi with get
    member val WebSocket = webSocket with get
    member val BufferSize = bufferSize with get

    interface IDisposable with
        member this.Dispose() = this.WebSocket.Dispose()

    /// <summary>
    /// Connects to the streaming API. \
    /// ストリーミング API に接続します．
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    member this.ConnectStreamingAsync(?cancellationToken: CancellationToken) =
        let cancellationToken = defaultArg cancellationToken CancellationToken.None

        // Add i parameter to the URI if the token is available.
        let withToken uri =
            this.HttpApi.Token
            |> Option.map (fun token -> uri |> UriMk.withParameter "i" token)
            |> Option.defaultValue uri

        // Make a URI for the WebSocket.
        let uri =
            UriMk(Wss, this.HttpApi.Host) |> UriMk.withDirectory "streaming" |> withToken

        let systemUri = System.Uri(uri.ToString())

        this.WebSocket.ConnectAsync(systemUri, cancellationToken)

    /// <summary>
    /// Receives a message from the streaming API. \
    /// ストリーミング API からメッセージを受信します．
    /// </summary>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    /// <returns>A message received from the streaming API. ストリーミング API から受信したメッセージ．</returns>
    member this.ReceiveAsync(?cancellationToken: CancellationToken) =
        let cancellationToken = defaultArg cancellationToken CancellationToken.None

        let buffer = Array.zeroCreate<byte> this.BufferSize

        let bufferSegment = ArraySegment(buffer)

        task {
            let! result = this.WebSocket.ReceiveAsync(bufferSegment, cancellationToken)

            let message =
                Encoding.UTF8.GetString(bufferSegment.Array, bufferSegment.Offset, result.Count)

            let messageDeserialized = JsonValue.Parse message

            return messageDeserialized
        }

    /// <summary>
    /// Sends a message to the streaming API. \
    /// ストリーミング API にメッセージを送信します．
    /// </summary>
    /// <param name="message">A message to send. 送信するメッセージ．</param>
    /// <param name="messageType">The type of the message. メッセージの種類．default: `WebSocketMessageType.Text`</param>
    /// <param name="endOfMessage">Whether the message is the end of a message. メッセージがメッセージの終わりかどうか．default: `true`</param>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    member this.SendAsync
        (
            message: JsonNode,
            ?messageType: WebSocketMessageType,
            ?endOfMessage: bool,
            ?cancellationToken: CancellationToken
        ) =
        let messageType = defaultArg messageType WebSocketMessageType.Text
        let endOfMessage = defaultArg endOfMessage true
        let cancellationToken = defaultArg cancellationToken CancellationToken.None

        let messageSerialized = message.ToString()

        let bytes = Encoding.UTF8.GetBytes(messageSerialized) |> ArraySegment

        this.WebSocket.SendAsync(bytes, messageType, endOfMessage, cancellationToken)


    /// <summary>
    /// Connect to a channel. \
    /// チャンネルに接続します．
    /// </summary>
    /// <param name="channel">A channel to connect. 接続するチャンネル．</param>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    /// <returns>A channel connection. チャンネル接続．</returns>
    member this.ConnectChannelAsync(channel: IChannel, ?cancellationToken: CancellationToken) =
        let channelConnection = ChannelConnection(channel)

        // {
        //     "type": "connect",
        //     "body": {
        //         "channel": "{channelName}",
        //         "id": "{channelConnection.Uuid}"
        //     }
        // }

        let mutable message = JsonObject()
        message.Add("type", "connect")
        let mutable body = JsonObject()
        body.Add("channel", channel.Name)
        body.Add("id", channelConnection.Uuid)
        message.Add("body", body)

        task {
            do! this.SendAsync(message, ?cancellationToken = cancellationToken)
            return channelConnection
        }

    /// <summary>
    /// Disconnects from a channel. \
    /// チャンネルから切断します．
    /// </summary>
    /// <param name="channelConnection">A channel connection to disconnect. 切断するチャンネル接続．</param>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    member this.DisconnectChannelAsync(channelConnection: ChannelConnection, ?cancellationToken: CancellationToken) =
        // {
        //     "type": "disconnect",
        //     "body": {
        //         "id": "{channelConnection.Uuid}"
        //     }
        // }

        let mutable message = JsonObject()
        message.Add("type", "disconnect")
        let mutable body = JsonObject()
        body.Add("id", channelConnection.Uuid)
        message.Add("body", body)

        task {
            do! this.SendAsync(message, ?cancellationToken = cancellationToken)
            return ()
        }

    /// <summary>
    /// Captures a note. \
    /// ノートをキャプチャします．
    /// </summary>
    /// <param name="noteId">A note ID to subscribe. 購読するノート ID．</param>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    /// <returns>An object that contains the note ID. ノート ID を含むオブジェクト．</returns>
    member this.SubNoteAsync(noteId: string, ?cancellationToken: CancellationToken) =
        // {
        //     "type": "subNote",
        //     "body": {
        //         "id": "{noteId}",
        //     }
        // }

        let mutable message = JsonObject()
        message.Add("type", "subNote")
        let mutable body = JsonObject()
        body.Add("id", noteId)
        message.Add("body", body)

        task {
            do! this.SendAsync(message, ?cancellationToken = cancellationToken)
            return NoteSubscription(noteId)
        }

    /// <summary>
    /// Uncaptures a note. \
    /// ノートのキャプチャを解除します．
    /// </summary>
    /// <param name="noteId">A note ID to unsubscribe. 購読解除するノート ID．</param>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    member this.UnsubNoteAsync(noteId: string, ?cancellationToken: CancellationToken) =
        // {
        //     "type": "unsubNote",
        //     "body": {
        //         "id": "{noteId}",
        //     }
        // }

        let mutable message = JsonObject()
        message.Add("type", "unsubNote")
        let mutable body = JsonObject()
        body.Add("id", noteId)
        message.Add("body", body)

        task {
            do! this.SendAsync(message, ?cancellationToken = cancellationToken)
            return ()
        }
