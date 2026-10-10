using System.Net;
using System.Reflection;
using satisfactory_server.Framework.Attributes;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace satisfactory_server.Framework.Handlers
{
internal class ControllerHandler : Handler
{
public override async Task HandleRequest(HttpListenerContext context)
{
string path = context.Request.Url?.AbsolutePath ?? "/";

path = path.Trim('/');

string serverPath = context.Request.Url?.Segments.Skip(1).FirstOrDefault()?.Trim('/') ?? "";

if (path.StartsWith(serverPath + "/", StringComparison.OrdinalIgnoreCase))
{
path = path.Substring(serverPath.Length + 1);
}

var assembly = Assembly.GetExecutingAssembly();

var controller = assembly.GetTypes().Where(type => Attribute.IsDefined(type,typeof(HttpControllerAttribute))).FirstOrDefault
(type =>
{ var attribute = type.GetCustomAttribute<HttpControllerAttribute>();
return attribute != null && (path.Equals(attribute.Route, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(attribute.Route + "/", StringComparison.OrdinalIgnoreCase));
        });

if (controller == null)
{
    if (Successor != null)
    {
        await Successor.HandleRequest(context);
    }
    else
    {
        context.Response.StatusCode = 404;
        context.Response.Close();
    }
    return;
}


// Console.WriteLine($"Найден контроллер: {controller.Name}");

var controllerAttribute = controller.GetCustomAttribute<HttpControllerAttribute>();

string methodPath = path.Substring( controllerAttribute!.Route.Length).Trim('/');
            //Console.WriteLine("Путь до контроллера: " + methodPath);
            //Console.WriteLine("HTTP-method: " + context.Request.HttpMethod);

var method = controller.GetMethods().FirstOrDefault(m =>
    {
        if (context.Request.HttpMethod == "GET")
        {
            var attribute = m.GetCustomAttribute<GetAttribute>();

            return attribute != null &&
                   attribute.Route.Equals(
                       methodPath,
                       StringComparison.OrdinalIgnoreCase);
        }

        if (context.Request.HttpMethod == "POST")
        {
            var attribute = m.GetCustomAttribute<PostAttribute>();

            return attribute != null &&
                   attribute.Route.Equals(
                       methodPath,
                       StringComparison.OrdinalIgnoreCase);
        }

        return false;
    });


            if (method == null)
            {
                Console.WriteLine($"Контроллер найден, но метод не найден.");
                Console.WriteLine($"Путь: '{path}'");
                Console.WriteLine($"Путь метода: '{methodPath}'");
                Console.WriteLine($"HTTP-метод: '{context.Request.HttpMethod}'");

                if (Successor != null)
                {
                    await Successor.HandleRequest(context);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.Close();
                }

                return;
            }
            // Console.WriteLine($"Найден метод: {method.Name}");

            var instance = Activator.CreateInstance(controller);

object? result;

if (context.Request.HttpMethod == "GET")
{
    result = method.Invoke(instance, null);
}
else if (context.Request.HttpMethod == "POST")
{
    using var reader = new StreamReader(
        context.Request.InputStream,
        context.Request.ContentEncoding
    );

    string body = await reader.ReadToEndAsync();
                //Console.WriteLine("тело post запроса: " + body);

    var formData = body
        .Split('&')
        .Select(item => item.Split('=', 2))
        .Where(parts => parts.Length == 2)
        .ToDictionary(
            parts => WebUtility.UrlDecode(parts[0]),
            parts => WebUtility.UrlDecode(parts[1]),
            StringComparer.OrdinalIgnoreCase
        );

    var parameters = method.GetParameters();

    object?[] arguments = parameters
        .Select(parameter =>
            formData.TryGetValue(parameter.Name!, out var value)
                ? (object?)value
                : null
        )
        .ToArray();

    result = method.Invoke(instance, arguments);
}
else
{
    context.Response.StatusCode = 405;
    context.Response.Close();
    return;
}

if (result is string responseText)
{
    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseText);

    context.Response.ContentType = "text/html; charset=utf-8";
    context.Response.ContentLength64 = buffer.Length;

    await context.Response.OutputStream.WriteAsync(buffer);
}

context.Response.StatusCode = 200;
context.Response.Close();
}
}
}