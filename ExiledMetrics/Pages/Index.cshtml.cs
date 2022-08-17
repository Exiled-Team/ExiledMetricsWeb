using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExiledMetrics.Pages;

using ExiledMetrics.Objects;

[IgnoreAntiforgeryToken(Order = 1001)]
public class IndexModel : PageModel
{
    internal string Message { get; set; }
    internal static Dictionary<string, Server> KnownServers { get; } = new();
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    
    public void OnPost()
    {
        string serverId = Request.Query["srvId"];
        Server? server;
        if (KnownServers.ContainsKey(serverId))
        {
            server = KnownServers[serverId];
        }
        else
        {
            server = new Server(serverId);
            KnownServers.Add(serverId, server);
        }

        server.ExiledVersion = Request.Query["exiled"];
        server.Players = int.Parse(Request.Query["players"]);
        server.PlayerLimit = int.Parse(Request.Query["playerMax"]);
        server.UpdateTpsValue(double.Parse(Request.Query["tps"]));
        if (Request.Query["team"] != "None")
            server.AwardTeamWin(Request.Query["team"]);
        server.ParsePlugins(Request.Query["plugins"]);

        Message += $"Server ID: {server.Id}\n";
        Message += $"Exiled Version: {server.ExiledVersion}\n";
        Message += $"Players: {server.Players}\n";
        Message += $"Player Limit: {server.PlayerLimit}\n";
        Message += $"TPS: {server.AverageFps}\n";
        Message += "Team Wins:\n";
        foreach (KeyValuePair<string, int> kvp in server.TeamWins)
            Message += $"- {kvp.Key}: {kvp.Value}\n";

        Message += "Plugins:\n";
        foreach (Plugin plugin in server.Plugins)
        {
            Message += "- ";
            Message += $"Name: {plugin.Name}\n";
            Message += $"  Version: {plugin.Version}\n";
            Message += $"  Author: {plugin.Author}\n";
        }

        
        Response.StartAsync();
        Response.WriteAsync(Message);
        Response.CompleteAsync();
    }
}