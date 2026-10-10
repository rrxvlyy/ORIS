using System.Net;

namespace satisfactory_server.Framework;

public class ControllerBase
{
    protected HttpListenerContext Context { get; private set; }
    
    internal void SetContext(HttpListenerContext context)
    {
        Context = context;
    }
}