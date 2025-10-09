using APISegaAI.Extensions;
using APISegaAI.Middleware.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHttpClients(builder.Configuration);
builder.Services.AddLogging(logging => logging.AddConsole());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Sega AI");
    });
}

app.UseErrorHandlerMiddleware();
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Применение миграций базы данных
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SegaAIContext>();
        if (!context.Database.CanConnect())
        {
            Console.WriteLine("БД не существует. Создание миграций и базы данных...");
            context.Database.Migrate();
            Console.WriteLine("БД успешно создана и миграции применены.");
        }
        else
        {
            context.Database.Migrate();
            Console.WriteLine("База данных существует, миграции применены.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при создании/применении миграций: {ex.Message}");
        throw;
    }
}

app.Run();