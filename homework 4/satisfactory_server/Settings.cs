namespace satisfactory_server;

public class Settings
{
    public ServerConfig Server { get; set; } = new();
}

public class ServerConfig
{
    public string Port { get; set; } = "9999";
    public string Host { get; set; } = "127.0.0.1";
    public string Path { get; set; } = "/";
}