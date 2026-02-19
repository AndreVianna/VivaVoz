namespace VivaVoz.Data;

public sealed class AppDbContext : DbContext {
    public DbSet<Recording> Recordings => Set<Recording>();
    public DbSet<Settings> Settings => Set<Settings>();

    public AppDbContext() {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) {
    }

    public static string GetDatabasePath() {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "VivaVoz", "data", "vivavoz.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (optionsBuilder.IsConfigured) {
            return;
        }

        var databasePath = GetDatabasePath();
        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory)) {
            Directory.CreateDirectory(databaseDirectory);
        }

        optionsBuilder.UseSqlite($"Data Source={databasePath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        var recording = modelBuilder.Entity<Recording>();
        recording.ToTable("Recordings");
        recording.HasKey(r => r.Id);
        recording.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);
        recording.Property(r => r.AudioFileName)
            .IsRequired();
        recording.Property(r => r.Transcript);
        recording.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();
        recording.Property(r => r.Language)
            .IsRequired()
            .HasDefaultValue("auto");
        recording.Property(r => r.Duration)
            .IsRequired();
        recording.Property(r => r.CreatedAt)
            .IsRequired();
        recording.Property(r => r.UpdatedAt)
            .IsRequired();
        recording.Property(r => r.WhisperModel)
            .IsRequired();
        recording.Property(r => r.FileSize)
            .IsRequired();

        var settings = modelBuilder.Entity<Settings>();
        settings.ToTable("Settings");
        settings.HasKey(s => s.Id);
        settings.Property(s => s.HotkeyConfig)
            .IsRequired();
        settings.Property(s => s.WhisperModelSize)
            .IsRequired()
            .HasDefaultValue("tiny");
        settings.Property(s => s.AudioInputDevice);
        settings.Property(s => s.StoragePath)
            .IsRequired();
        settings.Property(s => s.ExportFormat)
            .IsRequired()
            .HasDefaultValue("MP3");
        settings.Property(s => s.Theme)
            .IsRequired()
            .HasDefaultValue("System");
        settings.Property(s => s.Language)
            .IsRequired()
            .HasDefaultValue("auto");
        settings.Property(s => s.AutoUpdate)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
