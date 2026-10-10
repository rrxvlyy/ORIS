namespace satisfactory_server.Framework.Atributes;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class GetAttribute : Attribute
{
    public string Route { get; init; }
    public GetAttribute(string route)
    {
        Route = route;
    }

}