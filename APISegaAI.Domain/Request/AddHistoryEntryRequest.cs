namespace APISegaAI.Domain.Request
{
    public record struct AddHistoryEntryRequest(
        [Required(ErrorMessage = "ID тест-кейса не может быть пустым")]
        [MinLength(1, ErrorMessage = "ID тест-кейса не может быть пустым")]
        string TestCaseId,
        [Required(ErrorMessage = "Действие не может быть пустым")]
        [MinLength(1, ErrorMessage = "Действие не может быть пустым")]
        [MaxLength(100, ErrorMessage = "Действие не может превышать 100 символов")]
        string Action,
        [Required(ErrorMessage = "Пользователь не может быть пустым")]
        [MinLength(1, ErrorMessage = "Пользователь не может быть пустым")]
        [MaxLength(100, ErrorMessage = "Пользователь не может превышать 100 символов")]
        string User,
        [MaxLength(1000, ErrorMessage = "Детали не могут превышать 1000 символов")]
        string? Details 
    );
}