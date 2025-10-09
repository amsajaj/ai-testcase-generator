namespace APISegaAI.Domain.ValueObjects
{
    public record ValidationResult(bool IsValid , string? Recommendation);
}