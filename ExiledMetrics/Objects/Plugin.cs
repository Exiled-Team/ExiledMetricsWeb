namespace ExiledMetrics.Objects;

public class Plugin
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }

    public Plugin(string name, string version, string author)
    {
        Name = name;
        Version = version;
        Author = author;
    }
}