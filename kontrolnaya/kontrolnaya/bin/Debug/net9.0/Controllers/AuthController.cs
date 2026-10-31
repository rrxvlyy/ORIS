using kontrolnaya.Framework.Attributes;

namespace kontrolnaya.Controllers
{
    [HttpController("auth")]
    internal class AuthController
    {
        [Get("login")]
        public string login()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "static", "login.html");
            return File.Exists(path) ? File.ReadAllText(path) : "login.html не найден";
        }

        [Post("login")]
        public string login(string login, string password)
        {
            Console.WriteLine($"[AUTH] login = '{login}', password = '{password}'");
            return $"OK: {login}";
        }
    }
}