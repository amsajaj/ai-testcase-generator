namespace APISegaAI.Service.Helper
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                throw new JsonException("Дата не может быть пустой");

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            if (DateTime.TryParseExact(dateString, "yyyy MM dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return date;

            throw new JsonException($"Невозможно преобразовать '{dateString}' в DateTime");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}