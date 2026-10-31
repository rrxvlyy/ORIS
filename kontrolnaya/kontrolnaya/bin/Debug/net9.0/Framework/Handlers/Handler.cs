using System.Net;

namespace kontrolnaya.Framework.Handlers;

public abstract class Handler
{
    public Handler? Successor { get; set; }
    public abstract Task HandleRequest(HttpListenerContext context);

    protected static async Task WriteTextAsync(HttpListenerResponse response, int code, string text)
    {
        response.StatusCode = code;
        response.ContentType = "text/plain; charset=utf-8";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(text);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();
    }
    
    protected static async Task WriteHtmlAsync(HttpListenerResponse response, int code, string html)
    {
        response.StatusCode = code;
        response.ContentType = "text/html; charset=utf-8";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();
    }
}