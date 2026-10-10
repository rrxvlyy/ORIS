using System.Net;
using System.Threading.Tasks;

namespace MyHttpServer.Framework.Handlers
{
    internal abstract class Handler
    {
        protected Handler? Successor { get; set; }
        public void SetSuccessor(Handler successor)
        {
            Successor = successor;
        }
        public abstract Task HandleRequest(HttpListenerContext context);
    }

}