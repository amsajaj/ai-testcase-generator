namespace APISegaAI.DAL
{
    public class SegaAIContext : DbContext
    {
        public SegaAIContext(DbContextOptions<SegaAIContext> options) : base(options) =>
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        public DbSet<TestCase> TestCases { get; set; } = null!;
        public DbSet<TestStep> TestSteps { get; set; } = null!;
        public DbSet<InputData> InputData { get; set; } = null!;
        public DbSet<HistoryEntry> HistoryEntries { get; set; } = null!;
        public DbSet<DataPool> DataPools { get; set; } = null!;
        public DbSet<DataPoolItem> DataPoolItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация TestCase
            modelBuilder.Entity<TestCase>(entity =>
            {
                entity.ToTable("TestCases");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Number).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreationDate).HasColumnType("timestamp with time zone");
                entity.Property(e => e.Name).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Author).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Precondition).HasMaxLength(2000);
                entity.Property(e => e.Postcondition).HasMaxLength(2000);
                entity.Property(e => e.TestCode).HasMaxLength(10000);
                entity.Property(e => e.Status)
                    .HasConversion(new EnumToStringConverter<TestCaseStatus>())
                    .HasMaxLength(20)
                    .IsRequired();

                // Индексы для оптимизации
                entity.HasIndex(e => e.Number).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            // Конфигурация TestStep
            modelBuilder.Entity<TestStep>(entity =>
            {
                entity.ToTable("TestSteps");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.TestCaseId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.StepNumber).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.ExpectedResult).HasMaxLength(1000).IsRequired();

                // Связь один-ко-многим с TestCase
                entity.HasOne(e => e.TestCase)
                    .WithMany(e => e.Steps)
                    .HasForeignKey(e => e.TestCaseId)
                    .OnDelete(DeleteBehavior.Cascade); // Каскадное удаление
            });

            // Конфигурация InputData
            modelBuilder.Entity<InputData>(entity =>
            {
                entity.ToTable("InputData");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(100).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                entity.Property(e => e.TestCaseId).HasMaxLength(36);

                // Связь один-к-одному (опциональная) с TestCase
                entity.HasOne(e => e.TestCase)
                    .WithOne()
                    .HasForeignKey<InputData>(e => e.TestCaseId)
                    .OnDelete(DeleteBehavior.SetNull); // При удалении TestCase, TestCaseId становится null
            });

            // Конфигурация HistoryEntry
            modelBuilder.Entity<HistoryEntry>(entity =>
            {
                entity.ToTable("HistoryEntries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.TestCaseId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Timestamp).HasColumnType("timestamp with time zone");
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.User).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Details);

                // Связь один-ко-многим с TestCase
                entity.HasOne(e => e.TestCase)
                    .WithMany(e => e.History)
                    .HasForeignKey(e => e.TestCaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Индекс для поиска по времени
                entity.HasIndex(e => e.Timestamp);
            });

            // Конфигурация DataPool
            modelBuilder.Entity<DataPool>(entity =>
            {
                entity.ToTable("DataPools");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Source)
                    .HasConversion(new EnumToStringConverter<DataPoolSource>())
                    .HasMaxLength(20)
                    .IsRequired();
                entity.Property(e => e.TestCaseId).HasMaxLength(36);

                // Связь один-к-одному (опциональная) с TestCase
                entity.HasOne(e => e.TestCase)
                    .WithOne()
                    .HasForeignKey<DataPool>(e => e.TestCaseId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Конфигурация DataPoolItem
            modelBuilder.Entity<DataPoolItem>(entity =>
            {
                entity.ToTable("DataPoolItems");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(36).IsRequired();
                entity.Property(e => e.DataPoolId).HasMaxLength(36).IsRequired();
                entity.Property(e => e.Data).IsRequired(); // JSON-строка

                // Связь один-ко-многим с DataPool
                entity.HasOne(e => e.DataPool)
                    .WithMany(e => e.Items)
                    .HasForeignKey(e => e.DataPoolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}