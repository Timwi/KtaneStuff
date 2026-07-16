using CsQuery;
using RT.Json;
using RT.Servers;
using RT.Util.ExtensionMethods;

namespace KtaneStuff;

internal static class Translatable
{
    private static object _lock = new();

    public static void RunServer()
    {
        var server = new HttpServer(8992);
#if DEBUG
        server.PropagateExceptions = true;
#endif

        //*
        var path = @"D:\c\KTANE\Public\HTML\wire placement.html";
        /*/
        var avoid = "embellished;translated;condensed;cheat".Split(';');
        var files = new DirectoryInfo(@"D:\c\KTANE\Public\HTML").EnumerateFiles("*.html").Where(f => avoid.All(av => !f.Name.Contains(av)) && File.ReadAllText(f.FullName).Apply(txt => !txt.Contains("class='translated'") && !txt.Contains(@"class=""translated""")));
        var path = files.PickRandom().FullName;
        /**/

        const string jsFile = @"D:\c\KTANE\KtaneStuff\Src\TranslatableJs.js";

        var doc = CQ.CreateDocument(File.ReadAllText(path));

        server.Handler = new UrlResolver(
            new UrlMapping(path: "/", specificPath: true, handler: req =>
            {
                doc["head"].Append(@"
                        <style id='translatable-style'>
                            .translatable {
                                background-color: hsl(60, 80%, 80%);
                            }
                            .translatable-mod {
                                background-color: hsl(0, 80%, 80%)
                            }
                        </style>");
                doc["body"].Append($@"<script id='translatable-script'>{File.ReadAllText(jsFile)}</script>");
                var htmlRaw = doc.Render();
                doc["#translatable-style"].Remove();
                doc["#translatable-script"].Remove();
                return HttpResponse.Html(htmlRaw);
            }),
            new UrlMapping(path: "/css", handler: new FileSystemHandler(@"D:\c\KTANE\Public\HTML\css").Handle),
            new UrlMapping(path: "/img", handler: new FileSystemHandler(@"D:\c\KTANE\Public\HTML\img").Handle),
            new UrlMapping(path: "/websocket", handler: req => HttpResponse.WebSocket(new translatableWebSocket(doc, path)))
        ).Handle;
        server.StartListening(true);
    }

    private class translatableWebSocket(CQ doc, string path) : WebSocket
    {
        public CQ Doc { get; private set; } = doc;
        public string Path { get; private set; } = path;

        protected override void OnTextMessageReceived(string msg)
        {
            lock (_lock)
            {
                var lst = JsonList.Parse(msg);
                var elem = Doc[lst[0].GetString()];
                if (lst[1] != null)
                    elem.RemoveClass(lst[1].GetString());
                if (lst[2] != null)
                    elem.AddClass(lst[2].GetString());
                save();
            }
        }

        private void save()
        {
            lock (_lock)
            {
                var txt = Doc.Render(OutputFormatters.Create(HtmlEncoders.MinimumNbsp));
                txt = txt.RegexReplace(@"\s*</head>", Environment.NewLine + "</head>");
                txt = txt.RegexReplace(@"\s*</body>", Environment.NewLine + "</body>");
                File.WriteAllText(Path, txt);
            }
        }
    }
}
