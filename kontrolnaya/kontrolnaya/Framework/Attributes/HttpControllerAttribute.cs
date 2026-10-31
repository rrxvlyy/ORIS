namespace kontrolnaya.Framework.Attributes
{
    public class HttpControllerAttribute: Attribute
    {
        public string Route { get; init; }
        
        public HttpControllerAttribute(string route)
        {
            Route = route;
        }
    }
}