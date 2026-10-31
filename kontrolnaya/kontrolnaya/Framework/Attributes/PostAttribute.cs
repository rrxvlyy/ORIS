namespace kontrolnaya.Framework.Attributes
{
    public class PostAttribute : Attribute
    {
        public string Route { get; init; }
        public PostAttribute(string route)
        {
            Route = route;
        }
    }
}