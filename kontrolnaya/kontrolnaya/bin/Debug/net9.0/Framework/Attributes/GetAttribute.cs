namespace kontrolnaya.Framework.Attributes
{
    public class GetAttribute : Attribute
    {
        public string Route { get; init; }
        public GetAttribute(string route)
        {
            Route = route;
        }
    }
}