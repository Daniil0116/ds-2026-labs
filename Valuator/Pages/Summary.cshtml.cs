using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator.Pages;
public class SummaryModel : PageModel
{
    private readonly ILogger<SummaryModel> _logger;
    private static readonly ConnectionMultiplexer redis = 
        ConnectionMultiplexer.Connect("localhost:6379");
    private readonly IDatabase _db = redis.GetDatabase();

    public SummaryModel(ILogger<SummaryModel> logger)
    {
        _logger = logger;
    }

    public double Rank { get; set; }
    public double Similarity { get; set; }
    public string Text { get; set; } = string.Empty;

    public void OnGet(string id)
    {
        _logger.LogDebug(id);

        // TODO: (pa1) проинициализировать свойства Rank и Similarity значениями из БД (Redis)
        Text = _db.StringGet($"TEXT-{id}").ToString(); 
        Rank = double.TryParse(_db.StringGet($"RANK-{id}").ToString(), out var r) ? r : 0.0;
        Similarity = double.TryParse(_db.StringGet($"SIMILARITY-{id}").ToString(), out var s) ? s : 0.0;
    }
}