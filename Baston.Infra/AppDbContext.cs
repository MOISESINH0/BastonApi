using Baston.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Baston.Infra;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<GpsFix> GpsFixes => Set<GpsFix>();
    public DbSet<AlertEvent> AlertEvents => Set<AlertEvent>();
    public DbSet<KvMetaServer> KvMetaServers => Set<KvMetaServer>();

    // 👇 Nueva tabla
    public DbSet<ConfianzaRequest> ConfianzaRequests => Set<ConfianzaRequest>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.HasPostgresExtension("pgcrypto");

        // usuario
        b.Entity<AppUser>(e =>
        {
            e.ToTable("usuario");
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId)
                .HasColumnName("usuario_id")
                .HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Email).HasColumnName("email");
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasColumnName("contrasena_hash");
            e.Property(x => x.FullName).HasColumnName("nombre_completo");

            e.Property(x => x.Rol)
                .HasColumnName("rol")
                .HasColumnType("text");

            // 👇 Nuevo campo Tag
            e.Property(x => x.UserTag)
                .HasColumnName("tag")
                .HasColumnType("text");

            e.HasIndex(x => x.UserTag).IsUnique();

            e.Property(x => x.IsActive).HasColumnName("activo");
            e.Property(x => x.CreatedAt).HasColumnName("creado_utc");
        });

        // dispositivo
        b.Entity<Device>(e =>
        {
            e.ToTable("dispositivo");
            e.HasKey(x => x.DeviceId);
            e.Property(x => x.DeviceId)
                .HasColumnName("dispositivo_id")
                .HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.OwnerUserId).HasColumnName("usuario_id");
            e.Property(x => x.BtAddress).HasColumnName("direccion_bt");
            e.Property(x => x.Model).HasColumnName("modelo");
            e.Property(x => x.Firmware).HasColumnName("firmware");
            e.Property(x => x.IsActive).HasColumnName("activo");
            e.Property(x => x.CreatedAt).HasColumnName("creado_utc");
            e.HasIndex(x => new { x.OwnerUserId, x.BtAddress })
                .IsUnique()
                .HasDatabaseName("u_dispositivo_usuario_bt");
        });

        // posicion_gps
        b.Entity<GpsFix>(e =>
        {
            e.ToTable("posicion_gps");
            e.HasKey(x => x.FixId);
            e.Property(x => x.FixId).HasColumnName("posicion_id");
            e.Property(x => x.DeviceId).HasColumnName("dispositivo_id");
            e.Property(x => x.T).HasColumnName("ts_utc");
            e.Property(x => x.Lat).HasColumnName("lat");
            e.Property(x => x.Lon).HasColumnName("lon");
            e.Property(x => x.AltM).HasColumnName("alt_m");
            e.Property(x => x.InsertedAt).HasColumnName("insertado_utc");

            e.HasIndex(x => new { x.DeviceId, x.T }).IsUnique()
                .HasDatabaseName("u_posicion_dispositivo_ts");
            e.HasIndex(x => x.T).HasDatabaseName("idx_posicion_ts");

            e.ToTable(tb =>
            {
                tb.HasCheckConstraint("ck_pos_lat", "lat BETWEEN -90 AND 90");
                tb.HasCheckConstraint("ck_pos_lon", "lon BETWEEN -180 AND 180");
            });
        });

        // evento_alerta
        b.Entity<AlertEvent>(e =>
        {
            e.ToTable("evento_alerta");
            e.HasKey(x => x.AlertId);
            e.Property(x => x.AlertId).HasColumnName("evento_id");
            e.Property(x => x.DeviceId).HasColumnName("dispositivo_id");
            e.Property(x => x.T).HasColumnName("ts_utc");
            e.Property(x => x.Type).HasColumnName("tipo").HasMaxLength(20);
            e.Property(x => x.Detail).HasColumnName("detalle").HasColumnType("jsonb");
            e.Property(x => x.InsertedAt).HasColumnName("insertado_utc");

            e.HasIndex(x => new { x.DeviceId, x.T, x.Type }).IsUnique()
                .HasDatabaseName("u_evento_dispositivo_ts_tipo");
            e.HasIndex(x => x.T).HasDatabaseName("idx_evento_ts");
        });

        // kv_meta_servidor
        b.Entity<KvMetaServer>(e =>
        {
            e.ToTable("kv_meta_servidor");
            e.HasKey(x => new { x.UserId, x.K });
            e.Property(x => x.UserId).HasColumnName("usuario_id");
            e.Property(x => x.K).HasColumnName("k");
            e.Property(x => x.V).HasColumnName("v");
            e.Property(x => x.UpdatedAt).HasColumnName("actualizado_utc");
        });

        // confianza_request
        b.Entity<ConfianzaRequest>(e =>
        {
            e.ToTable("confianza_request");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("request_id")
                .HasDefaultValueSql("gen_random_uuid()");

            e.Property(x => x.SenderId).HasColumnName("remitente_id");
            e.Property(x => x.ReceiverId).HasColumnName("receptor_id");
            e.Property(x => x.SentAt).HasColumnName("enviado_utc");
            e.Property(x => x.Status)
                .HasColumnName("estado")
                .HasMaxLength(20);

            e.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
