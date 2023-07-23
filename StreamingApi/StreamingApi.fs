namespace StreamingApi

// TODO: Use a kind of ClientWebSocketFactory instead of ClientWebSocket.

open Uri
open HttpApi
open System.Net.WebSockets
open System.Threading
open System
open System.Text
open System.Text.Json.Nodes

// TODO: 別ファイルに分ける．

type IChannel =
    abstract member Name: string

type GlobalTimelineChannel() =
    interface IChannel with
        member __.Name = "globalTimeline"

type HomeTimelineChannel() =
    interface IChannel with
        member __.Name = "homeTimeline"

type HybridTimelineChannel() =
    interface IChannel with
        member __.Name = "hybridTimeline"

type LocalTimelineChannel() =
    interface IChannel with
        member __.Name = "localTimeline"

type MainChannel() =
    interface IChannel with
        member __.Name = "main"

//

type ChannelConnection() =
    member val internal Uuid = Guid.NewGuid().ToString() with get

type StreamingApi(httpApi: HttpApi, webSocket: ClientWebSocket, ?bufferSize: int) =
    static member private DEFAULT_BUFFER_SIZE = 65536

    member val HttpApi = httpApi with get
    member val WebSocket = webSocket with get
    member val BufferSize = defaultArg bufferSize StreamingApi.DEFAULT_BUFFER_SIZE with get

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
            |> Option.map (fun token -> uri |> Uri.withParameter "i" token)
            |> Option.defaultValue uri

        // Make a URI for the WebSocket.
        let uri =
            Uri.Mk(Wss, this.HttpApi.Host) |> Uri.withDirectory "streaming" |> withToken

        let systemUri = System.Uri(uri.ToString())

        this.WebSocket.ConnectAsync(systemUri, cancellationToken) |> Async.AwaitTask

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

        async {
            let! result = this.WebSocket.ReceiveAsync(bufferSegment, cancellationToken) |> Async.AwaitTask

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
        |> Async.AwaitTask


    /// <summary>
    /// Connect to a channel. \
    /// チャンネルに接続します．
    /// </summary>
    /// <param name="channel">A channel to connect. 接続するチャンネル．</param>
    /// <param name="cancellationToken">The cancellation token. キャンセルトークン．default: `CancellationToken.None`</param>
    /// <returns>A channel connection. チャンネル接続．</returns>
    member this.ConnectChannelAsync(channel: IChannel, ?cancellationToken: CancellationToken) =
        let channelConnection = ChannelConnection()

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

        async {
            do! this.SendAsync(message, ?cancellationToken = cancellationToken)
            return channelConnection
        }
