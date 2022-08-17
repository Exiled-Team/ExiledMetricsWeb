using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExiledMetricsWeb;

using System.Globalization;

public class metrics : PageModel
{
    public void OnGet()
    {
        string dateTimme = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        ViewData["TimeStamp"] = dateTimme;
    }
}