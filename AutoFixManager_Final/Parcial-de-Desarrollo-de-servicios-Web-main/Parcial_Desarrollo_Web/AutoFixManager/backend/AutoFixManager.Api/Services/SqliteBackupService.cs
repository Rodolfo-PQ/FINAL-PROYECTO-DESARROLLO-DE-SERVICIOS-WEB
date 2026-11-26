using System.Data;
using AutoFixManager.Api.Data;
using AutoFixManager.Api.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AutoFixManager.Api.Services;

public sealed class SqliteBackupService(
    AutoFixDbContext source,
    IConfiguration configuration,
    IWebHostEnvironment environment) : ISqliteBackupService
{
    public async Task<SqliteBackupResult> CreateBackupAsync(CancellationToken cancellationToken)
    {
        var configuredPath = configuration["Backup:SqlitePath"] ?? Path.Combine("backups", "autofixmanager-backup.sqlite");
        var backupPath = Path.GetFullPath(
            Path.IsPathRooted(configuredPath) ? configuredPath : Path.Combine(environment.ContentRootPath, configuredPath));
        var directory = Path.GetDirectoryName(backupPath)
            ?? throw new InvalidOperationException("No se pudo determinar la carpeta del respaldo SQLite.");
        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(directory, $"{Path.GetFileName(backupPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            List<Models.Cliente> clientes;
            List<Models.Usuario> usuarios;
            List<Models.Vehiculo> vehiculos;
            List<Models.Servicio> servicios;
            List<Models.ServicioMecanico> serviciosMecanicos;
            List<Models.RefreshTokenRegistro> refreshTokens;
            List<Models.Cita> citas;
            List<Models.CategoriaRepuesto> categoriasRepuesto;
            List<Models.Repuesto> repuestos;
            List<Models.Proveedor> proveedores;
            List<Models.OrdenCompra> ordenesCompra;
            List<Models.DetalleOrdenCompra> detallesOrdenCompra;
            List<Models.MovimientoInventario> movimientosInventario;
            List<Models.ServicioRepuesto> serviciosRepuestos;
            List<Models.HistorialEstadoServicio> historialEstadosServicio;

            await using (var transaction = await source.Database.BeginTransactionAsync(
                IsolationLevel.RepeatableRead, cancellationToken))
            {
                clientes = await source.Clientes.AsNoTracking().ToListAsync(cancellationToken);
                usuarios = await source.Usuarios.AsNoTracking().ToListAsync(cancellationToken);
                vehiculos = await source.Vehiculos.AsNoTracking().ToListAsync(cancellationToken);
                servicios = await source.Servicios.AsNoTracking().ToListAsync(cancellationToken);
                serviciosMecanicos = await source.ServiciosMecanicos.AsNoTracking().ToListAsync(cancellationToken);
                refreshTokens = await source.RefreshTokens.AsNoTracking().ToListAsync(cancellationToken);
                citas = await source.Citas.AsNoTracking().ToListAsync(cancellationToken);
                categoriasRepuesto = await source.CategoriasRepuesto.AsNoTracking().ToListAsync(cancellationToken);
                repuestos = await source.Repuestos.AsNoTracking().ToListAsync(cancellationToken);
                proveedores = await source.Proveedores.AsNoTracking().ToListAsync(cancellationToken);
                ordenesCompra = await source.OrdenesCompra.AsNoTracking().ToListAsync(cancellationToken);
                detallesOrdenCompra = await source.DetallesOrdenCompra.AsNoTracking().ToListAsync(cancellationToken);
                movimientosInventario = await source.MovimientosInventario.AsNoTracking().ToListAsync(cancellationToken);
                serviciosRepuestos = await source.ServiciosRepuestos.AsNoTracking().ToListAsync(cancellationToken);
                historialEstadosServicio = await source.HistorialEstadosServicio.AsNoTracking().ToListAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = temporaryPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();
            var options = new DbContextOptionsBuilder<SqliteBackupDbContext>()
                .UseSqlite(connectionString)
                .Options;

            await using (var destination = new SqliteBackupDbContext(options))
            {
                await destination.Database.EnsureCreatedAsync(cancellationToken);
                destination.Clientes.AddRange(clientes);
                destination.Usuarios.AddRange(usuarios);
                destination.Vehiculos.AddRange(vehiculos);
                destination.Servicios.AddRange(servicios);
                destination.ServiciosMecanicos.AddRange(serviciosMecanicos);
                destination.RefreshTokens.AddRange(refreshTokens);
                destination.Citas.AddRange(citas);
                destination.CategoriasRepuesto.AddRange(categoriasRepuesto);
                destination.Repuestos.AddRange(repuestos);
                destination.Proveedores.AddRange(proveedores);
                destination.OrdenesCompra.AddRange(ordenesCompra);
                destination.DetallesOrdenCompra.AddRange(detallesOrdenCompra);
                destination.MovimientosInventario.AddRange(movimientosInventario);
                destination.ServiciosRepuestos.AddRange(serviciosRepuestos);
                destination.HistorialEstadosServicio.AddRange(historialEstadosServicio);
                await destination.SaveChangesAsync(cancellationToken);
            }

            File.Move(temporaryPath, backupPath, overwrite: true);

            return new SqliteBackupResult(
                Path.GetFileName(backupPath),
                DateTimeOffset.UtcNow,
                new Dictionary<string, int>
                {
                    ["Clientes"] = clientes.Count,
                    ["Usuarios"] = usuarios.Count,
                    ["Vehiculos"] = vehiculos.Count,
                    ["Servicios"] = servicios.Count,
                    ["ServiciosMecanicos"] = serviciosMecanicos.Count,
                    ["RefreshTokens"] = refreshTokens.Count,
                    ["Citas"] = citas.Count,
                    ["CategoriasRepuesto"] = categoriasRepuesto.Count,
                    ["Repuestos"] = repuestos.Count,
                    ["Proveedores"] = proveedores.Count,
                    ["OrdenesCompra"] = ordenesCompra.Count,
                    ["DetallesOrdenCompra"] = detallesOrdenCompra.Count,
                    ["MovimientosInventario"] = movimientosInventario.Count,
                    ["ServiciosRepuestos"] = serviciosRepuestos.Count,
                    ["HistorialEstadosServicio"] = historialEstadosServicio.Count
                });
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
