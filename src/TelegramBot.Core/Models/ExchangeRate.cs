using Newtonsoft.Json;

namespace TelegramBot.Core.Models;

public class ExchangeRate
{
    [JsonProperty("baseCurrency")]
    public string? BaseCurrency { get; set; }

    [JsonProperty("currency")]
    public string? Currency { get; set; }

    [JsonProperty("saleRateNB")]
    public decimal SaleRateNb { get; set; }
    
    [JsonProperty("purchaseRateNB")]
    public decimal PurchaseRateNb { get; set; }

    [JsonProperty("saleRate")]
    public decimal SaleRate { get; set; }
    
    [JsonProperty("purchaseRate")]
    public decimal PurchaseRate { get; set; }
}