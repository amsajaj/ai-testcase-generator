namespace APISegaAI.Domain.Entity
{
    /// <summary>
    /// Сущность для хранения входных данных, используемых для генерации тест-кейсов.
    /// Хранит функциональные требования, сценарии, URL или другие технические документы,
    /// которые подаются на вход LLM для создания или обновления тест-кейсов.
    /// Может быть связана с конкретным TestCase для повторной генерации или валидации.
    /// Обеспечивает traceability от исходных данных к сгенерированному тест-кейсу,
    /// упрощая аудит и отладку процесса генерации.
    /// </summary>
    public class InputData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Уникальный идентификатор

        [Required]
        public string Content { get; set; } = string.Empty; // Текст или JSON (например, требования, сценарии, URL)

        [Required]
        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // Тип данных (например, "Requirements", "Scenario", "URL")

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Дата создания

        [ForeignKey(nameof(TestCase))]
        public string? TestCaseId { get; set; } // Внешний ключ для связи с TestCase (опционально)

        public TestCase? TestCase { get; set; } // Навигационное свойство (опционально)
    }
}