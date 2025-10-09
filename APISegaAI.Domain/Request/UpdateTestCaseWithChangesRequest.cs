namespace APISegaAI.Domain.Request
{
    public record UpdateTestCaseWithChangesRequest(
        [Required(ErrorMessage = "Данные изменений не могут быть пустыми")]
        [MinLength(1, ErrorMessage = "Данные изменений не могут быть пустыми")]
        string ChangesInput,

        [Required(ErrorMessage = "Модель LLM не может быть пустой")]
        [MinLength(1, ErrorMessage = "Модель LLM не может быть пустой")]
        [RegularExpression("^(qwen3-32b-awq|qwen3-30b-awq-4bit|gemma-3-27b-it-bnb-4bit|dbra-rag-qwen3-32b-awq)$", ErrorMessage = "Недопустимая модель LLM")]
        string LlmModel
    );
}