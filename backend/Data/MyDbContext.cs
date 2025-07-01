using Microsoft.EntityFrameworkCore;
using Simon.Movilidad.Api.Data.Entities;

namespace Simon.Movilidad.Api.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<SensorReading> SensorReadings { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<JwtBlacklist> JwtBlacklist { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // roles
            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("roles");
                e.HasKey(x => x.Id);
                e.Property(x => x.Name)
                    .HasMaxLength(20)
                    .IsRequired();
            });

            // users
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);
                e.Property(x => x.Username)
                    .HasMaxLength(50)
                    .IsRequired();
                e.Property(x => x.PasswordHash)
                    .HasMaxLength(200)
                    .IsRequired();
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                e.HasOne(x => x.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(x => x.RoleId);
            });

            // vehicles
            modelBuilder.Entity<Vehicle>(e =>
            {
                e.ToTable("vehicles");
                e.HasKey(x => x.Id);
                e.Property(x => x.Code)
                    .HasMaxLength(50)
                    .IsRequired();
                e.HasOne(x => x.Owner)
                    .WithMany(u => u.Vehicles)
                    .HasForeignKey(x => x.OwnerId);
                e.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // sensor_readings (guardamos lat/lon en columnas separadas)
            modelBuilder.Entity<SensorReading>(e =>
            {
                e.ToTable("sensor_readings");
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Vehicle)
                    .WithMany(v => v.SensorReadings)
                    .HasForeignKey(x => x.VehicleId);
                e.Property(x => x.Latitude).IsRequired();
                e.Property(x => x.Longitude).IsRequired();
                e.Property(x => x.FuelLevel).IsRequired();
                e.Property(x => x.Temperature).IsRequired();
                e.Property(x => x.RawPayload);
            });

            // alerts
          modelBuilder.Entity<Alert>(e =>
                {
                    e.ToTable("alerts");
                    e.HasKey(x => x.Id);

                    // Mapeamos el enum a un string o a un entero:
                    e.Property(x => x.Type)
                    .HasConversion<string>()         // o .HasConversion<int>()
                    .HasColumnName("type")
                    .IsRequired();

                    e.Property(x => x.Message).IsRequired();
                    e.Property(x => x.TriggeredAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                    e.Property(x => x.Acknowledged).HasDefaultValue(false);
                    e.Property(x => x.SeenByAdmin).HasDefaultValue(false);

                    e.HasOne(x => x.Vehicle)
                    .WithMany(v => v.Alerts)
                    .HasForeignKey(x => x.VehicleId);
                });


            // jwt_blacklist
            modelBuilder.Entity<JwtBlacklist>(e =>
            {
                e.ToTable("jwt_blacklist");
                e.HasKey(x => x.Id);
                e.Property(x => x.Jti)
                    .HasMaxLength(100)
                    .IsRequired();
                e.Property(x => x.RevokedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
