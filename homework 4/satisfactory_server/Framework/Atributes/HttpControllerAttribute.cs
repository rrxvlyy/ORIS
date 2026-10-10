namespace satisfactory_server.Framework.Atributes;
[AttributeUsage(AttributeTargets.Class)]
public class HttpControllerAttribute : Attribute
{
    public string Route { get; init; }
    
    public HttpControllerAttribute(string route)
    {
        Route = route;
    }
}