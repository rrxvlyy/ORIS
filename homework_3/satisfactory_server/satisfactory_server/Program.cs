using System.Text.Json;
using CustomHttpServer.Core;
using MyHttpServer;

string settingsJson = File.ReadAllText("settings.json");
Settings settings = JsonSerializer.Deserialize<Settings>(settingsJson)!;

var server = new HttpServer(settings);
server.Start();

Console.WriteLine("--- для завершения работы введите stop ---");
while (true)
{
    string? command = Console.ReadLine();
    if (command?.ToLower() == "stop")
    {
        server.Stop();
        break;
    }
}