namespace APISegaAI.Domain.Request
{
    public record struct GenerateDataPoolRequest(
        [property: Required(ErrorMessage = "TestCaseJson не может быть пустым")]
        [property: MinLength(1, ErrorMessage = "TestCaseJson не может быть пустым")]
        string TestCaseJson,
        
        [property: Required(ErrorMessage = "LlmModel не может быть пустым")]
        [property: MinLength(1, ErrorMessage = "LlmModel не может быть пустым")]
        [property: RegularExpression("^(qwen3-32b-awq|qwen3-30b-awq-4bit|gemma-3-27b-it-bnb-4bit|dbra-rag-qwen3-32b-awq)$", ErrorMessage = "Недопустимая модель LLM")]
        string LlmModel);
}