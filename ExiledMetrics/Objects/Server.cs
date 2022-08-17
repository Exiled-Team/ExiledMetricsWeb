namespace ExiledMetrics.Objects;

using System.Diagnostics;

public class Server
{
    public string Id { get; set; }
    public string ExiledVersion { get; set; }
    public int Players { get; set; }
    public int PlayerLimit { get; set; }
    public Dictionary<string, int> TeamWins { get; set; } = new();
    public double TotalTps { get; set; }
    public double AverageFps { get; set; }
    public List<Plugin> Plugins { get; set; } = new();
    public int Updates { get; set; }

    public void AwardTeamWin(string team)
    {
        if (!TeamWins.ContainsKey(team))
            TeamWins.Add(team, 0);
        TeamWins[team]++;
    }

    public void UpdateTpsValue(double value)
    {
        TotalTps += value;
        Updates++;
        AverageFps = TotalTps / Updates;
    }

    public Server(string id) => Id = id;

    internal void ParsePlugins(string plugins)
    {
        Plugins.Clear();
        foreach (string pluginInfo in plugins.Split("||"))
        {
            if (string.IsNullOrEmpty(pluginInfo))
                continue;
            
            string[] split = pluginInfo.Split('|');
            if (!bool.Parse(split[3]))
                continue;

            Plugins.Add(new Plugin(split[0], split[1], split[2]));
        }
    }
}