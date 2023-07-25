using Misskey.Net.Uri;
using Misskey.Net.HttpApi;
using Misskey.Net.StreamingApi;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;

//

// Name of this app.
var appName = "Example app";

// Hostname of Misskey instance.
var host = "misskey.systems";

//

var client =
    new ServiceCollection() // Create DI container.
        .AddHttpClient() // Add HttpClient to DI container.
        .BuildServiceProvider() // Build DI container.
        .GetService<IHttpClientFactory>(); // Get IHttpClientFactory from DI container.

// Create HttpApi instance.
var httpApi = new HttpApi(scheme: Misskey.Net.Uri.Scheme.Https, host: host, client: client);

//

// Get stats of Misskey instance
var stats = await httpApi.RequestApiAsync(new[] { "stats" });

Console.WriteLine($"stats: {stats}");

//

// Authorize this app to Misskey instance with permission `read:account` (but not required).
await httpApi.AuthorizeAsync(name: appName, permissions: new[] { Permission.NewRead(new PermissionKind.Account()) });

// Wait for authorization.
var check = await httpApi.WaitCheckAsync();

if (!check)
{
    Console.WriteLine("authorization failed");
}

Console.WriteLine("authorization completed");

//

// Create StreamingApi instance.
using var streamingApi = new StreamingApi(httpApi: httpApi, webSocket: new ClientWebSocket());

// Connect to Misskey instance.
await streamingApi.ConnectStreamingAsync();

Console.WriteLine("connected");

// Connect to global timeline.
var channelConnection = await streamingApi.ConnectChannelAsync(new Channel.GlobalTimeline());

while (true)
{
    // Subscribe to global timeline.
    var result = await streamingApi.ReceiveAsync();

    var content = result["body"];

    var body = content?["body"];

    if (body != null)
    {
        var user = body["user"];

        var nameNode = user?["name"];

        var name = (nameNode == null) ? "<unknown>" : nameNode.ToString();

        var renote = body["renote"];

        if (renote == null)
        {
            Console.WriteLine("-- text --");

            Console.WriteLine($"user: {name}");

            var text = body["text"];

            if (text != null)
            {
                Console.WriteLine($"text: {text}");
            }
            else
            {
                Console.WriteLine("text: <image only>");
            }
        }
        else
        {
            Console.WriteLine("-- renote --");

            var text = renote["text"];

            var renotedUser = renote["user"];

            var renotedNameNode = renotedUser?["name"];

            var renotedName = renotedNameNode == null ? "<unknown>" : renotedNameNode.ToString();

            Console.WriteLine($"user: {renotedName}");

            Console.WriteLine($"renoted by: {name}");

            if (text != null)
            {
                Console.WriteLine($"text: {text}");
            }
            else
            {
                Console.WriteLine("text: <image only>");
            }
        }
    }
    else
    {
        Console.WriteLine("unknown");
    }
}

