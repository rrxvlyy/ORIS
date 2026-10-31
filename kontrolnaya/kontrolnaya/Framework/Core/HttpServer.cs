using System.Net;
using kontrolnaya.Framework.Handlers;

namespace kontrolnaya;

public class HttpServer
{
    private readonly HttpListener _listener = new();
    private readonly string _url;
    private readonly string _rootDir;
    private bool _isRunning;
    private readonly Handler _chain;

    public HttpServer(string url, string rootDir)
    {
        _url = url;
        _rootDir = rootDir;

        var staticHandler = new StaticFileHandler();
        var controllerHandler = new ControllerHandler();
        staticHandler.Successor = controllerHandler;
        _chain = staticHandler;
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
        try { _listener.BeginGetContext(ListenerCallback, _listener); }
        catch (ObjectDisposedException) { }
        catch (HttpListenerException) { }
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        if (!_listener.IsListening) return;
        HttpListenerContext? context = null;
        try
        {
            context = _listener.EndGetContext(result);
            await _chain.HandleRequest(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            try
            {
                context!.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch { }
        }
        finally
        {
            Receive();
        }
    }

    private void TrySendError(HttpListenerContext? context, int code, string msg)
    {
        if (context == null) return;
        try
        {
            context.Response.StatusCode = code;
            context.Response.ContentType = "text/plain; charset=utf-8";
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{code}: {msg}");
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes);
            context.Response.Close();
        }
        catch { }
    }
}