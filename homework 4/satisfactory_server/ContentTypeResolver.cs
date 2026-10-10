namespace satisfactory_server;

public class ContentTypeResolver
{
    // Словарь: расширение файла -> MIME-тип
    private static readonly Dictionary<string, string> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        // ============ TEXT ============
        { ".html", "text/html; charset=utf-8" },
        { ".htm",  "text/html; charset=utf-8" },
        { ".css",  "text/css; charset=utf-8" },
        { ".js",   "application/javascript; charset=utf-8" },
        { ".mjs",  "application/javascript; charset=utf-8" },
        { ".json", "application/json; charset=utf-8" },
        { ".xml",  "application/xml; charset=utf-8" },
        { ".txt",  "text/plain; charset=utf-8" },
        { ".csv",  "text/csv; charset=utf-8" },
        { ".md",   "text/markdown; charset=utf-8" },

        // ============ IMAGE ============
        { ".png",  "image/png" },
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif",  "image/gif" },
        { ".svg",  "image/svg+xml" },
        { ".webp", "image/webp" },
        { ".ico",  "image/x-icon" },
        { ".bmp",  "image/bmp" },
        { ".tif",  "image/tiff" },
        { ".tiff", "image/tiff" },
        { ".avif", "image/avif" },

        // ============ AUDIO ============
        { ".mp3",  "audio/mpeg" },
        { ".wav",  "audio/wav" },
        { ".ogg",  "audio/ogg" },
        { ".oga",  "audio/ogg" },
        { ".m4a",  "audio/mp4" },
        { ".aac",  "audio/aac" },
        { ".flac", "audio/flac" },

        // ============ VIDEO ============
        { ".mp4",  "video/mp4" },
        { ".webm", "video/webm" },
        { ".ogv",  "video/ogg" },
        { ".avi",  "video/x-msvideo" },
        { ".mov",  "video/quicktime" },
        { ".mkv",  "video/x-matroska" },

        // ============ FONTS ============
        { ".woff",  "font/woff" },
        { ".woff2", "font/woff2" },
        { ".ttf",   "font/ttf" },
        { ".otf",   "font/otf" },
        { ".eot",   "application/vnd.ms-fontobject" },

        // ============ DOCUMENTS / ARCHIVES ============
        { ".pdf",  "application/pdf" },
        { ".zip",  "application/zip" },
        { ".rar",  "application/vnd.rar" },
        { ".7z",   "application/x-7z-compressed" },
        { ".tar",  "application/x-tar" },
        { ".gz",   "application/gzip" },
        { ".doc",  "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls",  "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".ppt",  "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },

        // ============ BINARY ============
        { ".bin",  "application/octet-stream" },
        { ".exe",  "application/octet-stream" },
        { ".dll",  "application/octet-stream" },
    };

    /// <summary>
    /// Возвращает MIME-тип для указанного файла.
    /// Если расширение неизвестно — возвращает "application/octet-stream".
    /// </summary>
    public static string GetContentType(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        return Types.TryGetValue(ext, out string? contentType)
            ? contentType
            : "application/octet-stream";
    }
}