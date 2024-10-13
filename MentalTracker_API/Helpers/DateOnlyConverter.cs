using System.Text.Json;
using System.Text.Json.Serialization;

namespace MentalTracker_API.Helpers
{
    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        private readonly string _format = "yyyy-MM-dd";
        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (DateOnly.TryParseExact(value, _format, null, System.Globalization.DateTimeStyles.None, out var date)) return date;
            }
            throw new JsonException("Неверный формат даты.");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>        
            writer.WriteStringValue(value.ToString(_format));
        
    }
}
