using Newtonsoft.Json;
using TelegramBot.Core.Models;

namespace TelegramBot.Core.Services;

public class ExchangeArchiveService
{
    private readonly HttpClient _httpClient;

    public ExchangeArchiveService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeArchive?> GetExchangeRate(DateTime dateTime)
    {
        var response = await _httpClient.GetStringAsync($"p24api/exchange_rates?json&date={dateTime:dd.MM.yyyy}");

        return JsonConvert.DeserializeObject<ExchangeArchive>(response);
    }
}