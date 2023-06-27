using Newtonsoft.Json;
using TelegramBot.Core.Converters;

namespace TelegramBot.Core.Models;

public class ExchangeArchive
{
    [JsonProperty("date")]
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonProperty("bank")] 
    public string Bank { get; set; } = "PB";

    [JsonProperty("baseCurrency")]
    public int BaseCurrency { get; set; }
    
    [JsonProperty("baseCurrencyLit")]
    public string? BaseCurrencyLit { get; set; }

    [JsonProperty("exchangeRate")]
    public IEnumerable<ExchangeRate>? ExchangeRates { get; set; }
}