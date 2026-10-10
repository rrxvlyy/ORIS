namespace satisfactory_server.Framework.Atributes;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PostAttribute
{
    public string Route { get; init; }
    public PostAttribute(string route)
    {
        Route = route;
    }
}