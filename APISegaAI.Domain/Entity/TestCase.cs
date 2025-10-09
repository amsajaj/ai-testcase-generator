namespace APISegaAI.Domain.Entity
{
    /// <summary>
    /// Сущность, представляющая тест-кейс, созданный вручную или с помощью LLM.
    /// Основная сущность приложения, которая хранит полную информацию о тест-кейсе,
    /// включая метаданные (номер, дата, название, автор), условия (пред- и постусловия),
    /// статус и коллекции связанных шагов и истории изменений.
    /// Используется для хранения, редактирования, генерации и экспорта тест-кейсов в различные форматы (Excel, Zephyr и т.д.).
    /// Обеспечивает основу для автоматизации тест-дизайна и отслеживания жизненного цикла тест-кейса.
    /// </summary>
    public class TestCase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // Уникальный идентификатор

        [Required]
        [MaxLength(50)]
        public string Number { get; set; } = string.Empty; // Номер тест-кейса (например, TC-001)

        [Required]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow; // Дата создания

        [Required]
        [MaxLength(500)]
        public string Name { get; set; } = string.Empty; // Название тест-кейса

        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = "AI Generated"; // Автор (AI или пользователь)

        [MaxLength(2000)]
        public string Precondition { get; set; } = string.Empty; // Предусловие

        [MaxLength(2000)]
        public string Postcondition { get; set; } = string.Empty; // Постусловие

        [Required]
        public TestCaseStatus Status { get; set; } = TestCaseStatus.Development; // Статус (Development/Active/Archive)

        public List<TestStep> Steps { get; set; } = new List<TestStep>(); // Коллекция шагов тест-кейса
        public List<HistoryEntry> History { get; set; } = new List<HistoryEntry>(); // История изменений

        [MaxLength(10000)]                
        public string TestCode {get; set;} = string.Empty; // Код теста
    } 
}