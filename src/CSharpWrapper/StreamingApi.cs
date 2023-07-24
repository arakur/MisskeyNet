using Misskey.Net.StreamingApi;
using Microsoft.FSharp.Core;
using System.Text.Json.Nodes;
using System.Net.WebSockets;

namespace Misskey.Net.StreamingApi
{
    public static class StreamingApiExtensions
    {
        public static Task ConnectStreamingAsync(this StreamingApi streamingApi, CancellationToken? cancellationToken = null)
        {
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.ConnectStreamingAsync(cancellationTokenOpt);
        }

        public static Task<JsonNode> ReceiveAsync(this StreamingApi streamingApi, CancellationToken? cancellationToken = null)
        {
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.ReceiveAsync(cancellationTokenOpt);
        }

        public static Task SendAsync(this StreamingApi streamingApi, JsonNode message, WebSocketMessageType? messageType = null, bool? endOfMessage = null, CancellationToken? cancellationToken = null)
        {
            FSharpOption<WebSocketMessageType> messageTypeOpt = messageType == null ? FSharpOption<WebSocketMessageType>.None : FSharpOption<WebSocketMessageType>.Some(messageType.Value);
            FSharpOption<bool> endOfMessageOpt = endOfMessage == null ? FSharpOption<bool>.None : FSharpOption<bool>.Some(endOfMessage.Value);
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.SendAsync(message, messageTypeOpt, endOfMessageOpt, cancellationTokenOpt);
        }
        public static Task<ChannelConnection> ConnectChannelAsync(this StreamingApi streamingApi, IChannel channel, CancellationToken? cancellationToken = null)
        {
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.ConnectChannelAsync(channel, cancellationTokenOpt);
        }

        public static Task DisconnectChannelAsync(this StreamingApi streamingApi, ChannelConnection channelConnection, CancellationToken? cancellationToken = null)
        {
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.DisconnectChannelAsync(channelConnection, cancellationTokenOpt);
        }

        public static Task<NoteSubscription> SubNoteAsync(this StreamingApi streamingApi, string noteId, CancellationToken? cancellationToken = null)
        {
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.SubNoteAsync(noteId, cancellationTokenOpt);
        }

        public static Task UnsubNoteAsync(this StreamingApi streamingApi, string noteId, CancellationToken? cancellationToken = null)
        {
            FSharpOption<CancellationToken> cancellationTokenOpt = cancellationToken == null ? FSharpOption<CancellationToken>.None : FSharpOption<CancellationToken>.Some(cancellationToken.Value);
            return streamingApi.UnsubNoteAsync(noteId, cancellationTokenOpt);
        }
    }
}