using Misskey.Net;
using Misskey.Net.HttpApi;
using Misskey.Net.StreamingApi;
using Misskey.Net.Permission;
using static Misskey.Net.ApiTypes;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using Microsoft.FSharp.Core;

//

// Name of this app.
var appName = "Example app";

// Hostname of Misskey instance.
var host = "misskey.systems";

//

// Main program:

// Create IHttpClientFactory instance.
var client =
    new ServiceCollection() // Create DI container.
        .AddHttpClient() // Add HttpClient to DI container.
        .BuildServiceProvider() // Build DI container.
        .GetService<IHttpClientFactory>(); // Get IHttpClientFactory from DI container.

// Create HttpApi instance.
var httpApi = new HttpApi(scheme: Misskey.Net.Uri.Scheme.Https, host: host, client: client);

// Get stats of Misskey instance.
var stats = new Stats(await httpApi.RequestApiAsync(new[] { "stats" }));

Console.WriteLine("-- stats --");

Console.WriteLine($"notes: {stats.NotesCount}");

Console.WriteLine($"users: {stats.UsersCount}");

Console.WriteLine($"instances: {stats.Instances}");

// 

// Authorize this app to Misskey instance with permission `read:notification`.
await httpApi.AuthorizeAsync(
    name: appName,
    permissions: new[] { Permission.NewRead(new PermissionKind.Notifications()) }
);

// Wait for authorization.
var auth = await httpApi.WaitCheckAsync();

if (auth == false)
    throw new Exception("authorization failed");

Console.WriteLine("authorization succeeded");

// 

// Create StreamingApi instance.
using var streamingApi = new StreamingApi(httpApi: httpApi, webSocket: new ClientWebSocket());

// Connect to Misskey instance.
await streamingApi.ConnectStreamingAsync();

Console.WriteLine("connected");

// Connect to local timeline.
var _channelConnection = await streamingApi.ConnectChannelAsync(new Misskey.Net.StreamingApi.Channel.LocalTimeline());

Console.WriteLine("connected to the local timeline channel");

// Connect to main channel.
var _channelConnection2 = await streamingApi.ConnectChannelAsync(new Misskey.Net.StreamingApi.Channel.Main());

Console.WriteLine("connected to the main channel");

while (true)
{
    Console.WriteLine("subscribing...");
    // Subscribe to global timeline.
    var result = await streamingApi.ReceiveAsync();

    switch (result)
    {
        // A note or a renote is posted.
        case StreamMessage.Channel channelMessage:
            var body = channelMessage.body.Body;

            switch (body)
            {
                case ChannelMessageBody.Note noteMessage:
                    var note = noteMessage.body;

                    var user = note.User;

                    // if user.Name is some then name = user.Name else name = "<no name>"
                    var name = FSharpOption<string>.get_IsSome(user.Name) ? user.Name.Value : "<no name>";
                    var text = FSharpOption<string>.get_IsSome(note.Text) ? note.Text.Value : "<image only>"; 

                    if (FSharpOption<Note>.get_IsNone(note.Renote))
                    {
                        Console.WriteLine("-- note --");
                        Console.WriteLine($"user: {name}");
                        Console.WriteLine($"text: {text}");
                    }
                    else
                    {
                        var renote = note.Renote.Value;
                        var original = FSharpOption<string>.get_IsSome(renote.User.Name) ? renote.User.Name.Value : "<no name>";
                        var renotedText = FSharpOption<string>.get_IsSome(renote.Text) ? renote.Text.Value : "<image only>";

                        Console.WriteLine("-- renote --");
                        Console.WriteLine($"user: {original}");
                        Console.WriteLine($"renoted by: {name}");
                        Console.WriteLine($"text: {renotedText}");
                    }
                    break;
                case ChannelMessageBody.Notification notificationMessage:
                    var notification = notificationMessage.body;
                    switch (notification.Body)
                    {
                        case NotificationModule.Body.OfReaction reactionBody:
                            var reaction = reactionBody.Item;
                            var reactingUser = FSharpOption<string>.get_IsSome(reaction.User.Name) ? reaction.User.Name.Value : "<no name>";
                            var reactedText = FSharpOption<string>.get_IsSome(reaction.Note.Text) ? reaction.Note.Text.Value : "<image only>";
                            Console.WriteLine("-- reaction --");
                            Console.WriteLine($"user: {reactingUser}");
                            Console.WriteLine($"reaction: {reaction.Reaction}");
                            Console.WriteLine($"text: {reactedText}");
                            break;
                        default:
                            Console.WriteLine("-- other notification --");
                            Console.WriteLine(notification.Body.ToString());
                            break;
                    }
                    break;
                default:
                    Console.WriteLine($"other message {body}");
                    break;
            }
            break;
        // Connected.
        case StreamMessage.Connected connectedMessage:
            var connected = connectedMessage.body;
            Console.WriteLine("-- connected --");
            Console.WriteLine($"id: {connected.Id}");
            break;
        // Note updated.
        case StreamMessage.NoteUpdated noteUpdatedMessage:
            var noteUpdated = noteUpdatedMessage.body;
            Console.WriteLine("-- note updated --");
            Console.WriteLine($"id: {noteUpdated.Id}");
            var bodyString = noteUpdated.BodyData.ToString().Substring(0, 30) + "...";
            Console.WriteLine($"body: {bodyString}");
            break;
        // Other messages.
        case StreamMessage.Other message:
            Console.WriteLine("-- other message --");
            break;
    }
}
