namespace APISegaAI.Domain.Entity
{
    /// <summary>
    /// Сущность для представления отдельного элемента тестовых данных (datapool).
    /// Хранит индивидуальные наборы данных в формате JSON (например, параметры для теста: email, пароль).
    /// Используется для параметризации тест-кейсов, позволяя запускать тесты с разными входными значениями.
    /// Связана с DataPool для группировки элементов в один пул данных.
    /// Поддерживает как автогенерацию через LLM, так и загрузку от пользователя.
    /// </summary>
    public class DataPoolItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Уникальный идентификатор

        [Required]
        [ForeignKey(nameof(DataPool))]
        public string DataPoolId { get; set; } = string.Empty; // Внешний ключ для связи с DataPool

        [Required]
        public string Data { get; set; } = string.Empty; // JSON-строка с данными (например, {"email": "test@example.com"})

        [Required]
        [JsonIgnore]
        public DataPool DataPool { get; set; } = null!; // Навигационное свойство
    }
}