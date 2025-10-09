namespace APISegaAI.Domain.Entity
{
    /// <summary>
    /// Сущность для хранения истории генерации или изменения тест-кейсов.
    /// Логирует все операции (генерация, редактирование, экспорт) с тест-кейсом,
    /// включая временные метки, тип действия, исполнителя и детали изменений.
    /// Позволяет пользователям просматривать историю для аудита, отладки и отслеживания эволюции тест-кейса.
    /// Связана с TestCase для обеспечения контекста и целостности лога.
    /// </summary>
    public class HistoryEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Уникальный идентификатор

        [Required]
        [ForeignKey(nameof(TestCase))]
        public string TestCaseId { get; set; } = string.Empty; // Внешний ключ для связи с TestCase

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Время операции

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // Тип действия (например, "Generated", "Edited", "Exported")

        [Required]
        [MaxLength(100)]
        public string User { get; set; } = "System"; // Кто выполнил (AI или пользователь)

        public string Details { get; set; } = string.Empty; // Дополнительные данные (например, JSON изменений)

        [JsonIgnore]
        [Required]
        public TestCase TestCase { get; set; } = null!; // Навигационное свойство
    }
}