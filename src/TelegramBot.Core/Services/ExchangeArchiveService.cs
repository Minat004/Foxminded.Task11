using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using TelegramBot.Core.Models;

namespace TelegramBot.Core.Services;

public class ExchangeArchiveService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExchangeArchiveService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ExchangeArchive?> GetExchangeRate(DateTime dateTime)
    {
        var uri = $"{_configuration["PrivatBank:ExchangeArchiveUrl"]}{dateTime:dd.MM.yyyy}";
        
        var response = await _httpClient.GetStringAsync(uri);

        return JsonConvert.DeserializeObject<ExchangeArchive>(response);
    }
}