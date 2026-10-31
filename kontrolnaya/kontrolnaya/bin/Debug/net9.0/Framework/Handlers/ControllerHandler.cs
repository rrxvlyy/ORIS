using System.Net;
using System.Reflection;
using System.Text;
using kontrolnaya.Framework.Attributes;

namespace kontrolnaya.Framework.Handlers
{
    internal class ControllerHandler : Handler
    {
        public override async Task HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string path = request.Url!.LocalPath;
            string[] segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

            bool isFile = Path.HasExtension(path);
            string controllerName = segments.Length > 0 ? segments[0] : "";

            if (!isFile)
            {
                var assembly = Assembly.GetExecutingAssembly();

                var controller = assembly.GetTypes()
                    .Where(t => Attribute.IsDefined(t, typeof(HttpControllerAttribute)))
                    .FirstOrDefault(c =>
                    {
                        var attr = c.GetCustomAttribute<HttpControllerAttribute>()!;
                        return string.Equals(attr.Route.Trim('/'), controllerName, StringComparison.OrdinalIgnoreCase);
                    });

                if (controller != null)
                {
                    string actionRoute = segments.Length > 1 ? string.Join("/", segments.Skip(1)) : "";

                    var method = controller.GetMethods()
                        .FirstOrDefault(m =>
                        {
                            var get  = m.GetCustomAttribute<GetAttribute>();
                            var post = m.GetCustomAttribute<PostAttribute>();
                            var put  = m.GetCustomAttribute<PutAttribute>();

                            if (request.HttpMethod == "GET" && get != null)
                                return string.Equals(get.Route.Trim('/'), actionRoute, StringComparison.OrdinalIgnoreCase);

                            if (request.HttpMethod == "POST" && post != null)
                                return string.Equals(post.Route.Trim('/'), actionRoute, StringComparison.OrdinalIgnoreCase);

                            if (request.HttpMethod == "PUT" && put != null)
                                return string.Equals(put.Route.Trim('/'), actionRoute, StringComparison.OrdinalIgnoreCase);

                            return false;
                        });

                    if (method != null)
                    {
                        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                        foreach (string? key in request.QueryString.AllKeys)
                            if (key != null) values[key] = request.QueryString[key] ?? "";

                        if ((request.HttpMethod == "POST" || request.HttpMethod == "PUT")
                            && request.HasEntityBody)
                        {
                            using var reader = new StreamReader(
                                request.InputStream,
                                request.ContentEncoding ?? Encoding.UTF8);
                            string body = await reader.ReadToEndAsync();

                            foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
                            {
                                var kv = pair.Split('=', 2);
                                string k = Uri.UnescapeDataString(kv[0].Replace('+', ' '));
                                string v = kv.Length > 1
                                    ? Uri.UnescapeDataString(kv[1].Replace('+', ' '))
                                    : "";
                                values[k] = v;
                            }
                        }
                        
                        var parameters = method.GetParameters();
                        object?[] args = new object?[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var p = parameters[i];
                            var targetType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;
                            bool isNullable = Nullable.GetUnderlyingType(p.ParameterType) != null
                                              || !p.ParameterType.IsValueType;

                            if (values.TryGetValue(p.Name!, out var raw))
                            {
                                if (string.IsNullOrEmpty(raw) && isNullable)
                                    args[i] = null;
                                else
                                    args[i] = Convert.ChangeType(raw, targetType);
                            }
                            else
                            {
                                args[i] = p.HasDefaultValue ? p.DefaultValue
                                                             : (isNullable ? null : Activator.CreateInstance(targetType));
                            }
                        }
                        
                        var instance = Activator.CreateInstance(controller);
                        object? ret = method.Invoke(instance, args);

                        if (ret is Task task)
                        {
                            await task;
                            ret = null;
                        }
                        
                        if (ret is string text)
                        {
                            if (text.StartsWith("__STATUS__"))
                            {
                                var parts = text.Split("__", 4, StringSplitOptions.None);
                                int code = int.Parse(parts[2]);
                                string payload = parts.Length > 3 ? parts[3] : "";

                                if (code == 303)
                                {
                                    response.StatusCode = 303;
                                    response.RedirectLocation = payload;
                                    response.Close();
                                }
                                else if (code == 204)
                                {
                                    response.StatusCode = 204;
                                    response.Close();
                                }
                                else
                                {
                                    await WriteHtmlAsync(response, code, payload);
                                }
                                return;
                            }
                            
                            await WriteHtmlAsync(response, 200, text);
                        }
                        else
                        {
                            response.Close();
                        }

                        return;
                    }
                }
            }

            if (Successor != null)
                await Successor.HandleRequest(context);
        }
    }
}