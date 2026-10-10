using System.Net;
using System.Text;

namespace satisfactory_server;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _url;
    private readonly string _rootDir;   // корневая папка со статикой ("static")
    private bool _isRunning;

    public HttpServer(string url, string rootDir)
    {
        _url = url;
        _rootDir = rootDir;
    }

    public void Start()
    {
        if (!Directory.Exists(_rootDir))
        {
            Console.WriteLine($"Ошибка: папка {_rootDir} не найдена!");
            return;
        }

        _listener.Prefixes.Add(_url);
        _listener.Start();
        _isRunning = true;
        Console.WriteLine($"Сервер запущен: {_url}");
        Console.WriteLine($"Корневая папка: {Path.GetFullPath(_rootDir)}");

        Receive();
    }

    public void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        _listener.Stop();
        Console.WriteLine("Сервер завершил работу.");
    }

    private void Receive()
    {
        if (!_isRunning) return;

        try
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }
        catch (ObjectDisposedException) { /* Сервер остановлен */ }
        catch (HttpListenerException)   { /* Сервер остановлен */ }
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;

        HttpListenerContext? context = null;
        try
        {
            context = _listener.EndGetContext(result);
            await HandleRequestAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки: {ex.Message}");
            TrySendError(context, 500, "Internal Server Error");
        }
        finally
        {
            Receive(); // Слушаем следующий запрос
        }
    }

    /// <summary>
    /// Определяет по URL, какой файл отдать.
    /// </summary>
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // 1. Путь из URL: "/about.html" или "/logo.png"
        string rawPath = request.Url!.AbsolutePath;

        // 2. Отрезаем префикс из настроек ("/" если Path пустой)
        string prefix = new Uri(_url).AbsolutePath; // "/"
        string relativePath = rawPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? rawPath.Substring(prefix.Length)
            : rawPath.TrimStart('/');

        // 3. Если запрошен корень — отдаем index.html
        if (string.IsNullOrEmpty(relativePath))
            relativePath = "site.html";

        // 4. Защита от path traversal ("../")
        relativePath = relativePath.Replace("..", "").TrimStart('/');

        // 5. Полный путь к файлу
        string fullPath = Path.Combine(_rootDir, relativePath);

        Console.WriteLine($"[REQ] {rawPath} → {fullPath}");

        // 6. Если файла нет — 404
        if (!File.Exists(fullPath))
        {
            Console.WriteLine($"[404] {relativePath}");
            await SendErrorAsync(response, 404, $"Файл '{relativePath}' не найден");
            return;
        }

        // 7. Отдаем файл
        await SendFileAsync(response, fullPath);
    }

    /// <summary>
    /// Отправляет файл с правильным Content-Type.
    /// </summary>
    private async Task SendFileAsync(HttpListenerResponse response, string fullPath)
    {
        try
        {
            response.ContentType = ContentTypeResolver.GetContentType(fullPath);

            byte[] buffer = await File.ReadAllBytesAsync(fullPath);
            response.ContentLength64 = buffer.Length;
            response.Headers.Add("Cache-Control", "no-cache");

            using (Stream output = response.OutputStream)
            {
                await output.WriteAsync(buffer);
                await output.FlushAsync();
            }

            Console.WriteLine($"[200] {Path.GetFileName(fullPath)} ({response.ContentType})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[500] {ex.Message}");
            await SendErrorAsync(response, 500, "Ошибка чтения файла");
        }
        finally
        {
            response.Close();
        }
    }

    /// <summary>
    /// Отправляет текстовую ошибку (404, 500).
    /// </summary>
    private async Task SendErrorAsync(HttpListenerResponse response, int statusCode, string message)
    {
        try
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/plain; charset=utf-8";

            byte[] buffer = Encoding.UTF8.GetBytes($"{statusCode}: {message}");
            response.ContentLength64 = buffer.Length;

            using (Stream output = response.OutputStream)
            {
                await output.WriteAsync(buffer);
            }
        }
        catch { /* ignore */ }
        finally
        {
            response.Close();
        }
    }

    private void TrySendError(HttpListenerContext? context, int code, string msg)
    {
        if (context == null) return;
        try
        {
            context.Response.StatusCode = code;
            context.Response.Close();
        }
        catch { /* ignore */ }
    }
}