namespace APISegaAI.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<SegaAIContext>(x => x.UseNpgsql(connectionString));

            services.AddScoped<ITestCaseRepository, TestCaseRepository>();
            services.AddScoped<ITestStepRepository, TestStepRepository>();
            services.AddScoped<IInputDataRepository, InputDataRepository>();
            services.AddScoped<IHistoryEntryRepository, HistoryEntryRepository>();
            services.AddScoped<IDataPoolRepository, DataPoolRepository>();
            services.AddScoped<IDataPoolItemRepository, DataPoolItemRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IDataPoolService, DataPoolService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<IInputDataService, InputDataService>();
            services.AddScoped<ILlmService, LlmService>();
            services.AddScoped<ITestCaseService, TestCaseService>();
            services.AddScoped(typeof(TestCaseGenerator<>));

            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:8383");
                });
            });
            
            return services;
        }
    }
}