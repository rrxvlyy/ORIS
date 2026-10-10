using System.Text.Json;
using Server_2;

// 1. Чтение настроек
string settingsJson = File.ReadAllText("settings.json");
Settings? 0 = JsonSerializer.Deserialize<Settings>(settingsJson);

if (settings?.Server == null)
{
    Console.WriteLine("Ошибка: не удалось загрузить настройки.");
    return;
}

// 2. Формируем URL (Path опционален)
string path = settings.Server.Path?.Trim('/') ?? "";
string uriPrefix = string.IsNullOrEmpty(path)
    ? $"http://{settings.Server.Host}:{settings.Server.Port}/"
    : $"http://{settings.Server.Host}:{settings.Server.Port}/{path}/";

// 3. Корневая папка со статикой
string rootDir = "static";

// 4. Создаем и запускаем сервер
var server = new HttpServer(uriPrefix, rootDir);
server.Start();

// 5. Управление через консоль
Console.WriteLine("Введите 'stop' для остановки сервера.");
while (true)
{
    string? command = Console.ReadLine();
    if (command?.ToLower() == "stop")
    {
        server.Stop();
        break;
    }
}