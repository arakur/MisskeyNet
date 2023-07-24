using Misskey.Net.HttpApi;
using Microsoft.FSharp.Core;
using System.Text.Json.Nodes;

namespace Misskey.Net.HttpApi
{
    public static class HttpApiExtensions
    {
        public static Task AuthorizeAsync(this HttpApi httpApi, string? name = null, string? icon = null, string? callback = null, IEnumerable<Permission>? permissions = null)
        {
            FSharpOption<string> nameOpt = name == null ? FSharpOption<string>.None : FSharpOption<string>.Some(name);
            FSharpOption<string> iconOpt = icon == null ? FSharpOption<string>.None : FSharpOption<string>.Some(icon);
            FSharpOption<string> callbackOpt = callback == null ? FSharpOption<string>.None : FSharpOption<string>.Some(callback);
            FSharpOption<IEnumerable<Permission>> permissionsOpt = permissions == null ? FSharpOption<IEnumerable<Permission>>.None : FSharpOption<IEnumerable<Permission>>.Some(permissions);
            return httpApi.AuthorizeAsync(nameOpt, iconOpt, callbackOpt, permissionsOpt);
        }

        public static Task<bool> WaitCheckAsync(this HttpApi httpApi, double? span = null, double? timeout = null, bool? silent = null)
        {
            FSharpOption<double> spanOpt = span == null ? FSharpOption<double>.None : FSharpOption<double>.Some(span.Value);
            FSharpOption<double> timeoutOpt = timeout == null ? FSharpOption<double>.None : FSharpOption<double>.Some(timeout.Value);
            FSharpOption<bool> silentOpt = silent == null ? FSharpOption<bool>.None : FSharpOption<bool>.Some(silent.Value);
            return httpApi.WaitCheckAsync(spanOpt, timeoutOpt, silentOpt);
        }

        public static Task<JsonNode> RequestApiAsync(this HttpApi httpApi, IEnumerable<string> endPointNameSeq, IEnumerable<(string, string)>? payload = null)
        {
            FSharpOption<IEnumerable<Tuple<string, string>>> payloadOpt =
                payload == null
                ? FSharpOption<IEnumerable<Tuple<string, string>>>.None
                : FSharpOption<IEnumerable<Tuple<string, string>>>.Some(payload.Select(x => Tuple.Create(x.Item1, x.Item2)));
            return httpApi.RequestApiAsync(endPointNameSeq, payloadOpt);
        }
    }
}