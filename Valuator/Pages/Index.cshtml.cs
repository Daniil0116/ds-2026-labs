using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private static readonly ConnectionMultiplexer redis = 
        ConnectionMultiplexer.Connect("localhost:6379"); 
    private readonly IDatabase _db = redis.GetDatabase();

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        if (string.IsNullOrWhiteSpace(text))
            return RedirectToPage("Index");

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        // TODO: (pa1) сохранить в БД (Redis) text по ключу textKey
        _db.StringSet(textKey, text, TimeSpan.FromHours(24)); 

        string rankKey = "RANK-" + id;
        // TODO: (pa1) посчитать rank и сохранить в БД (Redis) по ключу rankKey
         double rank = CalculateRank(text); 
        _db.StringSet(rankKey, rank.ToString(), TimeSpan.FromHours(24));

        string similarityKey = "SIMILARITY-" + id;
        // TODO: (pa1) посчитать similarity и сохранить в БД (Redis) по ключу similarityKey
        string textHash = GenerateHash(text);
        double similarity = _db.KeyExists($"DUPLICATE:{textHash}") ? 1.0 : 0.0;
        _db.StringSet(similarityKey, similarity.ToString(), TimeSpan.FromHours(24));

       if (similarity == 0)
            _db.StringSet($"DUPLICATE:{textHash}", "1", TimeSpan.FromHours(24));

        return RedirectToPage("Summary", new { id });
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0.0;
        
        int alphaChars = 0;
        foreach (char c in text)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                (c >= 'а' && c <= 'я') || (c >= 'А' && c <= 'Я') ||
                c == 'ё' || c == 'Ё')
            {
                alphaChars++;
            }
        }
        return 1.0 - (double)alphaChars / text.Length;
    }

    private string GenerateHash(string text)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
        return Convert.ToBase64String(hash).Substring(0, 16);
    }
}
