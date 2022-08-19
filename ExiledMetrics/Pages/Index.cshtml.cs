using System.Security.Cryptography;
using System.Text;
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
    private static readonly HMACSHA256 _hmacHandler = new(Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("secret") ?? ""));

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }
    
    public void OnPost()
    {
        // TODO: Add proper responses.

        string serverId = Request.Query["srvId"];
        string timestamp = Request.Query["timestamp"];
        string hmac = Request.Query["hmac"];

        if (string.IsNullOrEmpty(serverId) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(hmac) ||
            !long.TryParse(timestamp, out long parsedTimestamp) || !IsTimestampValid(parsedTimestamp) || !IsHmacValid(serverId, timestamp, hmac))
        {
            Response.StartAsync();
            Response.CompleteAsync();
            return;
        }

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

    private static bool IsTimestampValid(long timestamp)
    {
        long timestapNow = DateTimeOffset.Now.ToUnixTimeSeconds();
        
        // Timestamps must be 3 hours apart. To check, we'll subtract them and
        // see if the distance is less than 3 hours. It should also be > 0 (as
        // the timestamp should be from the past).
        return timestapNow - timestamp is > 0 and < 3 * 60 * 60;
    }

    private static bool IsHmacValid(string id, string timestamp, string hmac)
    {
        // The basic idea here is to create our own HMAC, then check if it's the same
        // as the one given to us by the server. Only us and Northwood have the secret key,
        // so there's no way for servers to fake anything.
        byte[] input = Encoding.ASCII.GetBytes(id + "-" + timestamp);
        string ourHmac = Convert.ToHexString(_hmacHandler.ComputeHash(input)).ToLower();

        return ourHmac == hmac;
    }
}