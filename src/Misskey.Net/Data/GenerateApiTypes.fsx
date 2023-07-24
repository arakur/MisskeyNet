// REMARK: This file is not complete and does not work.

open System.Net.Http
open System.Text.RegularExpressions

//

let url =
    "https://raw.githubusercontent.com/misskey-dev/misskey/7097d553e4d6f0e6273abe1710d133fe5194d628/packages/misskey-js/etc/misskey-js.api.md"

let mdWriteTo = "./misskey-js.api.md"

//

let apiMd =
    if System.IO.File.Exists(mdWriteTo) then
        System.IO.File.ReadAllText(mdWriteTo)
    else

        let client = new HttpClient()

        let apiMd = client.GetStringAsync(url).Result

        System.IO.File.WriteAllText(mdWriteTo, apiMd)

        apiMd

//

let pattern = @"```ts\s*([\s\S]*?)```"

let matches = Regex.Matches(apiMd, pattern)

let content = matches.[0].Groups.[1].Value

//

// TODO: Write program to generate API types from API document.
