using Newtonsoft.Json.Converters;

namespace TelegramBot.Core.Converters;

public class CustomDateTimeConverter : IsoDateTimeConverter
{
    public CustomDateTimeConverter()
    {
        DateTimeFormat = "dd.MM.yyyy";
    }
}