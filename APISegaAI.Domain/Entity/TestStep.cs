namespace APISegaAI.Domain.Entity
{
    /// <summary>
    /// Сущность, представляющая отдельный шаг тест-кейса.
    /// Используется для детализации последовательности действий в тест-кейсе.
    /// Каждый шаг включает описание выполняемого действия и ожидаемый результат,
    /// что позволяет четко структурировать процесс тестирования и облегчает автоматизацию или ручное выполнение.
    /// Связана с TestCase через внешний ключ для обеспечения целостности данных.
    /// </summary>
    public class TestStep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Уникальный идентификатор

        [Required]
        [ForeignKey(nameof(TestCase))]
        public string TestCaseId { get; set; } = string.Empty; // Внешний ключ для связи с TestCase

        [Required]
        public int StepNumber { get; set; } // Номер шага (1, 2, 3...)

        [Required]
        [MaxLength(1000)]
        public string Action { get; set; } = string.Empty; // Описание действия (например, "Ввести email")

        [Required]
        [MaxLength(1000)]
        public string ExpectedResult { get; set; } = string.Empty; // Ожидаемый результат

        [Required]
        [JsonIgnore]
        public TestCase TestCase { get; set; } = null!; // Навигационное свойство
    }
}