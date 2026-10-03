namespace CustomHttpServer.Core;

public static class MimeTypes
{
    private static readonly Dictionary<string, string> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=UTF-8",
        [".htm"]  = "text/html; charset=UTF-8",
        [".css"]  = "text/css; charset=UTF-8",
        [".js"]   = "application/javascript; charset=UTF-8",
        [".json"] = "application/json; charset=UTF-8",
        [".xml"]  = "application/xml",
        [".txt"]  = "text/plain; charset=UTF-8",
        [".png"]  = "image/png",
        [".jpg"]  = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"]  = "image/gif",
        [".svg"]  = "image/svg+xml",
        [".webp"] = "image/webp",
        [".ico"]  = "image/x-icon",
        [".woff"] = "font/woff",
        [".woff2"]= "font/woff2",
        [".ttf"]  = "font/ttf",
        [".pdf"]  = "application/pdf",
        [".mp3"]  = "audio/mpeg",
        [".mp4"]  = "video/mp4",
    };

    public static string Get(string extension)
        => _map.TryGetValue(extension, out var v) ? v : "application/octet-stream";
}