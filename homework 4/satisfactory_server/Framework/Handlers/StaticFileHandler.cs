using System.Net;
using System.Collections.Generic;

namespace MyHttpServer.Framework.Handlers
{
internal class StaticFileHandler : Handler
{
private readonly string staticPath;
private readonly string rootPath;
private readonly Dictionary<string, string> pages;

public StaticFileHandler(string staticPath, string rootPath, Dictionary<string, string> pages)
{
    this.staticPath = staticPath;
    this.rootPath = rootPath;
    this.pages = pages;
}
    
public override async Task HandleRequest(HttpListenerContext context)
{
    string requestPath = context.Request.Url?.AbsolutePath ?? "/";
    requestPath = requestPath.Trim('/');

    string[] segments = context.Request.Url?.Segments?? Array.Empty<string>();

    string serverPath = segments.Length > 1? segments[1].Trim('/'): "";

    if (!string.IsNullOrEmpty(serverPath) && requestPath.StartsWith(serverPath + "/", StringComparison.OrdinalIgnoreCase))
    {
        requestPath = requestPath.Substring(serverPath.Length + 1);
    }
    else if (requestPath.Equals(serverPath, StringComparison.OrdinalIgnoreCase))
    {
        requestPath = "";
    }

    string[] parts = requestPath.Split('/', 2);

    if (pages.ContainsKey(requestPath))
    {
        requestPath = pages[requestPath];
    }
    else if (parts.Length == 2 && pages.ContainsKey(parts[0]))
    {
        string page = pages[parts[0]];
        string pageFolder = Path.GetDirectoryName(page) ?? "";

        requestPath = Path.Combine(pageFolder, parts[1]);
    }

    string fullPath = Path.Combine(staticPath, requestPath);

    if (string.IsNullOrEmpty(requestPath) || !File.Exists(fullPath))
    {
        Console.WriteLine($"Файл не найден: {fullPath}");

        fullPath = Path.Combine(rootPath, "404.html");
        context.Response.StatusCode = 404;
    }
    else
    {
        context.Response.StatusCode = 200;
    }

    string extension = Path.GetExtension(fullPath).ToLowerInvariant();

    if (ContentTypes.Types.TryGetValue(extension, out string? contentType))
    {
        context.Response.ContentType = contentType;
    }
    else
    {
        context.Response.ContentType = "application/octet-stream";
    }

    byte[] buffer = await File.ReadAllBytesAsync(fullPath);

    context.Response.ContentLength64 = buffer.Length;

    using (Stream output = context.Response.OutputStream)
    {
        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    context.Response.Close();

    //Console.WriteLine($"Отправлен файл: {fullPath}");
}
}


}