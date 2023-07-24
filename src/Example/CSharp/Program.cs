// Console.WriteLine("TODO");

using Misskey.Net.Uri;
using Misskey.Net.HttpApi;
using Misskey.Net.StreamingApi;
using Misskey.Net.CSharpWrapper.HttpApi;

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
else
{
    Console.WriteLine("authorization completed");

    //

    // Create StreamingApi instance.
    using var streamingApi = new StreamingApi(httpApi: httpApi, webSocket: new ClientWebSocket());

    // Connect to Misskey instance.
    await streamingApi.ConnectStreamingAsync();

    Console.WriteLine("connected");

    // Connect to global timeline.
    var channelConnection = await streamingApi.ConnectChannelAsync(Channel.GlobalTimeline());

    while (true)
    {
        // Subscribe to global timeline.
        var result = await streamingApi.ReceiveAsync();

        var textNode = result["body"]["body"]["text"];

        if (textNode == null)
        {
            var renoteNode = result["body"]["body"]["renote"]["text"];
            Console.WriteLine($"renote: {renoteNode}");
        }
        else
        {
            Console.WriteLine($"text: {textNode}");
        }
    }
}