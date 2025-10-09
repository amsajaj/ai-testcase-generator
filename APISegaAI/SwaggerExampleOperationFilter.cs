using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace APISegaAI
{
    public class SwaggerExampleOperationFilter : IOperationFilter 
    { 
    
        public void Apply(OpenApiOperation operation, OperationFilterContext context) 
        { 
            // Обработка атрибута SwaggerOperationAttribute 
            var attribute = context.MethodInfo.GetCustomAttribute<SwaggerOperationAttribute>(); 
            if (attribute != null) 
            { 
                operation.Summary = attribute.Summary; 
                operation.Description = attribute.Description; 
            }

            // Извлечение XML-документации
            var xmlDoc = LoadXmlDocumentation(context.MethodInfo);
            if (xmlDoc == null) return;

            // Обработка тега <example>
            var exampleElement = xmlDoc.Element("example");
            if (exampleElement == null) return;

            var exampleContent = exampleElement.Value.Trim();
            // Разделение на запрос и ответ с учётом "Response ("
            var exampleParts = Regex.Split(exampleContent, @"Response\s*\(\d+\s*[^\)]+\):");
            if (exampleParts.Length < 1) return;

            // Обработка запроса
            if (exampleParts[0].Trim().Length > 0)
            {
                operation.RequestBody ??= new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>()
                };
                
                // Определяем MIME-тип на основе атрибутов метода
                var formAttr = context.MethodInfo.GetParameters()
                    .SelectMany(p => p.GetCustomAttributes())
                    .OfType<FromFormAttribute>()
                    .Any();
                
                var contentType = formAttr ? "multipart/form-data" : "application/json";
                
                // Парсим пример запроса как текст
                var requestExample = exampleParts[0].Trim();
                operation.RequestBody.Content[contentType] = new OpenApiMediaType
                {
                    Example = new OpenApiString(requestExample),
                    Schema = context.SchemaGenerator.GenerateSchema(
                        context.MethodInfo.GetParameters().FirstOrDefault()?.ParameterType ?? typeof(object),
                        context.SchemaRepository)
                };
            }

            // Обработка ответа
            if (exampleParts.Length > 1)
            {
                foreach (var responsePart in exampleParts.Skip(1))
                {
                    var responseContent = responsePart.Trim();
                    if (string.IsNullOrWhiteSpace(responseContent)) continue;

                    // Извлекаем статус-код из заголовка ответа (например, "200 OK")
                    var statusMatch = Regex.Match(responsePart, @"(\d+)\s*[^\s]+");
                    var statusCode = statusMatch.Success ? statusMatch.Groups[1].Value : "200";

                    if (!operation.Responses.ContainsKey(statusCode))
                    {
                        operation.Responses[statusCode] = new OpenApiResponse
                        {
                            Description = $"Response {statusCode}",
                            Content = new Dictionary<string, OpenApiMediaType>()
                        };
                    }

                    // Парсим JSON ответа, если возможно
                    try
                    {
                        var jsonExample = responseContent;
                        operation.Responses[statusCode].Content["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(jsonExample),
                            Schema = context.SchemaGenerator.GenerateSchema(
                                context.MethodInfo.ReturnType.GetGenericArguments().FirstOrDefault() ?? typeof(object),
                                context.SchemaRepository)
                        };
                    }
                    catch
                    {
                        // Если JSON невалиден, добавляем как текст
                        operation.Responses[statusCode].Content["application/json"] = new OpenApiMediaType
                        {
                            Example = new OpenApiString(responseContent)
                        };
                    }
                }
            }
        }

        private XElement? LoadXmlDocumentation(MemberInfo memberInfo)
        {
            var assembly = memberInfo.DeclaringType?.Assembly;
            var xmlFile = $"{assembly?.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"XML documentation file not found at: {xmlPath}");
                return null;
            }

            var xmlDoc = XDocument.Load(xmlPath);
            var memberName = $"M:{memberInfo.DeclaringType?.FullName}.{memberInfo.Name}";
            if (memberInfo is MethodInfo methodInfo)
            {
                var parameters = methodInfo.GetParameters();
                if (parameters.Length > 0)
                {
                    memberName += $"({string.Join(",", parameters.Select(p => p.ParameterType.FullName))})";
                }
            }
            var memberDoc = xmlDoc.Descendants("member")
                .FirstOrDefault(m => m.Attribute("name")?.Value == memberName);
            if (memberDoc == null)
            {
                Console.WriteLine($"No XML documentation found for member: {memberName}");
            }
            return memberDoc;
        }
    }
}