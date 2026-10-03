using System.Net;
using System.Text;
using MyHttpServer;

namespace CustomHttpServer.Core;

public class HttpServer
{
    private readonly Settings _settings;
    private readonly string _staticRoot;
    private HttpListener _listener;

    public HttpServer(Settings settings)
    {
        _settings = settings;
        _staticRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "static"));
    }

    public void Start()
    {
        if (!Directory.Exists(_staticRoot))
        {
            Console.WriteLine($"Ошибка: папка {_staticRoot} не найдена!");
            return;
        }

        string host = _settings.Server.Host;
        string port = _settings.Server.Port;

        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{host}:{port}/");
        _listener.Start();
        Console.WriteLine($"Сервер запущен: http://{host}:{port}/");

        Receive();
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
        Console.WriteLine("Сервер завершил свою работу");
    }

    private void Receive()
    {
        _listener.BeginGetContext(ListenerCallback, _listener);
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;

        var context = _listener.EndGetContext(result);
        var request = context.Request;
        var response = context.Response;

        Console.WriteLine($"{request.HttpMethod} {request.Url?.LocalPath}");

        try
        {
            // 1. Берём путь из URL
            string localPath = request.Url?.LocalPath ?? "/";

            // Если корень "/" — подставляем файл по умолчанию
            if (localPath == "/" || string.IsNullOrEmpty(localPath))
                localPath = "/" + _settings.Server.Path;

            // 2. Собираем путь к файлу
            string relative = Uri.UnescapeDataString(localPath).TrimStart('/');
            string fullPath = Path.Combine(_staticRoot, relative);

            // 3. Проверяем, что файл есть
            if (!File.Exists(fullPath))
            {
                string notFound = Path.Combine(_staticRoot, "404.html");
                if (File.Exists(notFound))
                {
                    response.StatusCode = 404;
                    response.ContentType = "text/html; charset=UTF-8";
                    byte[] err = await File.ReadAllBytesAsync(notFound);
                    response.ContentLength64 = err.Length;
                    await response.OutputStream.WriteAsync(err);
                    response.OutputStream.Close();
                }
                else
                {
                    await WriteTextAsync(response, 404, "404 Not Found");
                }
                return;
            }

            // 4. Content-Type через MimeTypes
            response.ContentType = MimeTypes.Get(Path.GetExtension(fullPath));
            response.StatusCode = 200;

            // 5. Отправляем файл
            byte[] buffer = await File.ReadAllBytesAsync(fullPath);
            response.ContentLength64 = buffer.Length;

            using Stream output = response.OutputStream;
            await output.WriteAsync(buffer);
            await output.FlushAsync();

            Console.WriteLine("Запрос обработан");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка: " + ex.Message);
            try { await WriteTextAsync(response, 500, "500 Internal Server Error"); }
            catch { }
        }
        finally
        {
            Receive();
        }
    }

    private static async Task WriteTextAsync(HttpListenerResponse response, int code, string text)
    {
        response.StatusCode = code;
        response.ContentType = "text/plain; charset=UTF-8";
        byte[] data = Encoding.UTF8.GetBytes(text);
        response.ContentLength64 = data.Length;
        await response.OutputStream.WriteAsync(data);
        response.OutputStream.Close();
    }
}