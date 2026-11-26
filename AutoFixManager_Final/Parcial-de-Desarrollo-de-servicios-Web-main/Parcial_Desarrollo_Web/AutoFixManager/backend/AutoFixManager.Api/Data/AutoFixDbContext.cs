using AutoFixManager.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace AutoFixManager.Api.Data;
public class AutoFixDbContext(DbContextOptions<AutoFixDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<ServicioMecanico> ServiciosMecanicos => Set<ServicioMecanico>();
    public DbSet<RefreshTokenRegistro> RefreshTokens => Set<RefreshTokenRegistro>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<CategoriaRepuesto> CategoriasRepuesto => Set<CategoriaRepuesto>();
    public DbSet<Repuesto> Repuestos => Set<Repuesto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<DetalleOrdenCompra> DetallesOrdenCompra => Set<DetalleOrdenCompra>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<ServicioRepuesto> ServiciosRepuestos => Set<ServicioRepuesto>();
    public DbSet<HistorialEstadoServicio> HistorialEstadosServicio => Set<HistorialEstadoServicio>();
    public DbSet<ServicioResumenEstado> ResumenesServicios => Set<ServicioResumenEstado>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, DateTime>(
            value => value.UtcDateTime,
            value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
        var nullableDateTimeOffsetConverter = new ValueConverter<DateTimeOffset?, DateTime?>(
            value => value.HasValue ? value.Value.UtcDateTime : null,
            value => value.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc))
                : null);

        model.Entity<Cliente>(e => { e.HasKey(x => x.Id); e.Property(x => x.Nombre).HasMaxLength(80).IsRequired(); e.Property(x => x.Apellido).HasMaxLength(80).IsRequired(); e.Property(x => x.Email).HasMaxLength(160).IsRequired(); e.HasIndex(x => x.Email).IsUnique(); e.Property(x => x.Telefono).HasMaxLength(30).IsRequired(); });
        model.Entity<Vehiculo>(e => { e.HasKey(x => x.Id); e.Property(x => x.Placa).HasMaxLength(12).IsRequired(); e.HasIndex(x => x.Placa).IsUnique(); e.Property(x => x.Marca).HasMaxLength(60); e.Property(x => x.Modelo).HasMaxLength(80); e.Property(x => x.Color).HasMaxLength(40); e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict); });
        model.Entity<Servicio>(e => { e.ToTable(t => t.HasCheckConstraint("CK_Servicios_CostoEstimado", "CostoEstimado >= 0")); e.HasKey(x => x.Id); e.Property(x => x.CostoEstimado).HasColumnType("decimal(12,2)"); e.Property(x => x.Descripcion).HasMaxLength(500); e.Property(x => x.TipoMantenimiento).HasMaxLength(80); e.Property(x => x.Estado).HasMaxLength(30); e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict); e.HasOne<Vehiculo>().WithMany().HasForeignKey(x => x.VehiculoId).OnDelete(DeleteBehavior.Restrict); e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.MecanicoId).IsRequired(false).OnDelete(DeleteBehavior.Restrict); e.HasIndex(x => new { x.ClienteId, x.Estado }); });
        model.Entity<Usuario>(e => { e.ToTable(t => t.HasCheckConstraint("CK_Usuarios_Rol", "Rol IN ('ADMIN','MECANICO','CLIENTE')")); e.HasKey(x => x.Id); e.Property(x => x.NombreUsuario).HasMaxLength(80).IsRequired(); e.HasIndex(x => x.NombreUsuario).IsUnique(); e.HasIndex(x => x.ClienteId).IsUnique(); e.Property(x => x.PasswordHash).IsRequired(); e.Property(x => x.Rol).HasMaxLength(20).IsRequired(); e.HasOne<Cliente>().WithOne().HasForeignKey<Usuario>(x => x.ClienteId).IsRequired(false).OnDelete(DeleteBehavior.Restrict); });
        model.Entity<ServicioMecanico>(e => { e.HasKey(x => new { x.ServicioId, x.MecanicoId }); e.Property(x => x.FechaAsignacion).HasConversion(dateTimeOffsetConverter).HasColumnType("datetime(6)"); e.HasOne(x => x.Servicio).WithMany(x => x.MecanicosAsignados).HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Cascade); e.HasOne(x => x.Mecanico).WithMany(x => x.ServiciosAsignados).HasForeignKey(x => x.MecanicoId).OnDelete(DeleteBehavior.Restrict); e.HasIndex(x => x.MecanicoId); });
        model.Entity<RefreshTokenRegistro>(e => { e.HasKey(x => x.Id); e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired(); e.HasIndex(x => x.TokenHash).IsUnique(); e.HasIndex(x => new { x.UsuarioId, x.RevocadoEn, x.ExpiraEn }); e.Property(x => x.CreadoEn).HasConversion(dateTimeOffsetConverter).HasColumnType("datetime(6)"); e.Property(x => x.ExpiraEn).HasConversion(dateTimeOffsetConverter).HasColumnType("datetime(6)"); e.Property(x => x.RevocadoEn).HasConversion(nullableDateTimeOffsetConverter).HasColumnType("datetime(6)"); e.Property(x => x.ReemplazadoPorHash).HasMaxLength(64); e.HasOne(x => x.Usuario).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade); });
        model.Entity<Cita>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.ClienteId);
            e.HasIndex(x => x.VehiculoId);
            e.HasIndex(x => x.ServicioId).IsUnique();
            e.HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Vehiculo>().WithMany().HasForeignKey(x => x.VehiculoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Servicio>().WithMany().HasForeignKey(x => x.ServicioId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
        model.Entity<CategoriaRepuesto>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(250);
            e.HasIndex(x => x.Nombre).IsUnique();
        });
        model.Entity<Repuesto>(e =>
        {
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Repuestos_Stock", "StockActual >= 0 AND StockMinimo >= 0");
                t.HasCheckConstraint("CK_Repuestos_Precios", "PrecioCosto >= 0 AND PrecioVenta >= 0");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).HasMaxLength(40).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(500);
            e.Property(x => x.PrecioCosto).HasColumnType("decimal(12,2)");
            e.Property(x => x.PrecioVenta).HasColumnType("decimal(12,2)");
            e.HasIndex(x => x.Codigo).IsUnique();
            e.HasIndex(x => x.CategoriaRepuestoId);
            e.HasOne<CategoriaRepuesto>().WithMany().HasForeignKey(x => x.CategoriaRepuestoId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<Proveedor>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Email).HasMaxLength(160);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.Direccion).HasMaxLength(250);
        });
        model.Entity<OrdenCompra>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_OrdenesCompra_Total", "Total >= 0"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.Property(x => x.Total).HasColumnType("decimal(12,2)");
            e.HasIndex(x => new { x.ProveedorId, x.FechaCreacion });
            e.HasOne<Proveedor>().WithMany().HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<DetalleOrdenCompra>(e =>
        {
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_DetallesOrdenCompra_Cantidad", "Cantidad > 0");
                t.HasCheckConstraint("CK_DetallesOrdenCompra_CostoUnitario", "CostoUnitario >= 0");
            });
            e.HasKey(x => new { x.OrdenCompraId, x.RepuestoId });
            e.Property(x => x.CostoUnitario).HasColumnType("decimal(12,2)");
            e.HasOne<OrdenCompra>().WithMany().HasForeignKey(x => x.OrdenCompraId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Repuesto>().WithMany().HasForeignKey(x => x.RepuestoId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<MovimientoInventario>(e =>
        {
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_MovimientosInventario_Tipo", "TipoMovimiento IN ('ENTRADA','SALIDA','AJUSTE')");
                t.HasCheckConstraint("CK_MovimientosInventario_Cantidad", "Cantidad > 0");
            });
            e.HasKey(x => x.Id);
            e.Property(x => x.TipoMovimiento).HasMaxLength(20).IsRequired();
            e.Property(x => x.Referencia).HasMaxLength(200);
            e.HasIndex(x => new { x.RepuestoId, x.FechaMovimiento });
            e.HasOne<Repuesto>().WithMany().HasForeignKey(x => x.RepuestoId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
        model.Entity<ServicioRepuesto>(e =>
        {
            e.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ServiciosRepuestos_Cantidad", "Cantidad > 0");
                t.HasCheckConstraint("CK_ServiciosRepuestos_PrecioUnitario", "PrecioUnitario >= 0");
            });
            e.HasKey(x => new { x.ServicioId, x.RepuestoId });
            e.Property(x => x.PrecioUnitario).HasColumnType("decimal(12,2)");
            e.HasOne<Servicio>().WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Repuesto>().WithMany().HasForeignKey(x => x.RepuestoId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<HistorialEstadoServicio>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EstadoAnterior).HasMaxLength(30);
            e.Property(x => x.EstadoNuevo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Comentario).HasMaxLength(500);
            e.HasIndex(x => new { x.ServicioId, x.FechaCambio });
            e.HasOne<Servicio>().WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });
        model.Entity<ServicioResumenEstado>().HasNoKey();
    }
}
