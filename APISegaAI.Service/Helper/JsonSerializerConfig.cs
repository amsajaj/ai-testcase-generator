namespace APISegaAI.Service.Helper
{
    public static class JsonSerializerConfig
    {
        public static JsonSerializerOptions Default => new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles, // для избежания бесконечных циклов
            WriteIndented = true, // для отладки
            PropertyNameCaseInsensitive = true, // для работы с JSON от внешних систем
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new CustomDateTimeConverter()
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static JsonSerializerOptions Minimal => new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static JsonSerializerOptions CreateCustom(Action<JsonSerializerOptions> configure)
        {
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            configure?.Invoke(options);
            return options;
        }
    }
}