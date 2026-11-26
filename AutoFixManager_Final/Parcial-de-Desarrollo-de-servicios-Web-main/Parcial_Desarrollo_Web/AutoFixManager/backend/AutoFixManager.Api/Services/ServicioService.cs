using AutoFixManager.Api.Data;
using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Middleware;
using AutoFixManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFixManager.Api.Services;

public sealed class ServicioService(AutoFixDbContext db) : IServicioService
{
    public IReadOnlyList<Servicio> GetAll() =>
        db.Servicios.AsNoTracking().Include(x => x.MecanicosAsignados)
            .OrderByDescending(x => x.FechaCreacion).ToList();

    public IReadOnlyList<Servicio> GetByCliente(int clienteId)
    {
        if (!db.Clientes.Any(x => x.Id == clienteId))
            throw new ApiException(404, $"No existe el cliente {clienteId}.");

        return db.Servicios.AsNoTracking().Include(x => x.MecanicosAsignados)
            .Where(x => x.ClienteId == clienteId)
            .OrderByDescending(x => x.FechaCreacion)
            .ToList();
    }

    public IReadOnlyList<Servicio> GetByMecanico(int mecanicoId)
    {
        EnsureMecanico(mecanicoId);
        return db.Servicios.AsNoTracking().Include(x => x.MecanicosAsignados)
            .Where(x => x.MecanicosAsignados.Any(a => a.MecanicoId == mecanicoId))
            .OrderByDescending(x => x.FechaCreacion)
            .ToList();
    }

    public IReadOnlyList<Cliente> GetClientesByMecanico(int mecanicoId)
    {
        EnsureMecanico(mecanicoId);
        return db.Clientes.AsNoTracking()
            .Where(cliente => db.Servicios.Any(servicio =>
                servicio.ClienteId == cliente.Id &&
                servicio.MecanicosAsignados.Any(a => a.MecanicoId == mecanicoId)))
            .OrderBy(cliente => cliente.Id)
            .ToList();
    }

    public IReadOnlyList<Vehiculo> GetVehiculosByMecanico(int mecanicoId)
    {
        EnsureMecanico(mecanicoId);
        return db.Vehiculos.AsNoTracking()
            .Where(vehiculo => db.Servicios.Any(servicio =>
                servicio.VehiculoId == vehiculo.Id &&
                servicio.MecanicosAsignados.Any(a => a.MecanicoId == mecanicoId)))
            .OrderBy(vehiculo => vehiculo.Id)
            .ToList();
    }

    public IReadOnlyList<Servicio> GetByEstado(string estado) =>
        db.Servicios.AsNoTracking().Include(x => x.MecanicosAsignados)
            .Where(x => x.Estado.ToLower() == estado.ToLower())
            .OrderByDescending(x => x.FechaCreacion)
            .ToList();

    public Servicio GetById(int id) =>
        db.Servicios.AsNoTracking().Include(x => x.MecanicosAsignados).FirstOrDefault(x => x.Id == id)
        ?? throw new ApiException(404, $"No existe el servicio {id}.");

    public Servicio GetByIdForMecanico(int id, int mecanicoId) =>
        db.Servicios.AsNoTracking().Include(x => x.MecanicosAsignados).FirstOrDefault(x =>
            x.Id == id && x.MecanicosAsignados.Any(a => a.MecanicoId == mecanicoId))
        ?? throw new ApiException(404, $"No existe el servicio asignado con identificador {id}.");

    public bool IsAssignedToMecanico(int servicioId, int mecanicoId) =>
        db.ServiciosMecanicos.AsNoTracking()
            .Any(x => x.ServicioId == servicioId && x.MecanicoId == mecanicoId);

    public Servicio Create(ServicioWriteDto input)
    {
        Validate(input);
        var servicio = Map(input, input.MecanicoId);
        SyncMecanicos(servicio, AssignedMechanicIds(input));
        db.Servicios.Add(servicio);
        Save();
        return servicio;
    }

    public Servicio CreateForMecanico(ServicioWriteDto input, int mecanicoId)
    {
        EnsureMecanico(mecanicoId);
        ValidateReferences(input);
        if (!db.Servicios.Any(x =>
                x.ClienteId == input.ClienteId &&
                x.VehiculoId == input.VehiculoId &&
                x.MecanicosAsignados.Any(a => a.MecanicoId == mecanicoId)))
            throw new ApiException(404, "El cliente y vehículo no están asignados a este mecánico.");

        var servicio = Map(input, mecanicoId);
        SyncMecanicos(servicio, [mecanicoId]);
        db.Servicios.Add(servicio);
        Save();
        return servicio;
    }

    public Servicio Update(int id, ServicioWriteDto input)
    {
        var servicio = db.Servicios.Include(x => x.MecanicosAsignados).FirstOrDefault(x => x.Id == id)
            ?? throw new ApiException(404, $"No existe el servicio {id}.");
        Validate(input);
        var existingLeadId = servicio.MecanicoId;
        ApplyChanges(servicio, input);
        servicio.MecanicoId = input.MecanicoId;
        SyncMecanicos(servicio, UpdatedMechanicIds(input, servicio, existingLeadId));
        Save();
        return servicio;
    }

    public Servicio UpdateForMecanico(int id, ServicioWriteDto input, int mecanicoId)
    {
        var servicio = db.Servicios.Include(x => x.MecanicosAsignados).FirstOrDefault(x =>
            x.Id == id && x.MecanicosAsignados.Any(a => a.MecanicoId == mecanicoId))
            ?? throw new ApiException(404, $"No existe el servicio asignado con identificador {id}.");
        if (servicio.ClienteId != input.ClienteId || servicio.VehiculoId != input.VehiculoId)
            throw new ApiException(400, "El mecánico no puede cambiar el cliente ni el vehículo de una orden.");

        ValidateReferences(input);
        ApplyChanges(servicio, input);
        Save();
        return servicio;
    }

    public void Delete(int id)
    {
        var servicio = db.Servicios.Find(id)
            ?? throw new ApiException(404, $"No existe el servicio {id}.");
        db.Servicios.Remove(servicio);
        Save();
    }

    private void Validate(ServicioWriteDto input)
    {
        ValidateReferences(input);
        foreach (var assignedMechanicId in AssignedMechanicIds(input))
            EnsureMecanico(assignedMechanicId);
    }

    private void ValidateReferences(ServicioWriteDto input)
    {
        if (!db.Clientes.Any(x => x.Id == input.ClienteId))
            throw new ApiException(404, $"No existe el cliente {input.ClienteId}.");

        var vehiculo = db.Vehiculos.FirstOrDefault(x => x.Id == input.VehiculoId)
            ?? throw new ApiException(404, $"No existe el vehículo {input.VehiculoId}.");
        if (vehiculo.ClienteId != input.ClienteId)
            throw new ApiException(400, "El vehículo seleccionado no pertenece al cliente indicado.");
    }

    private void EnsureMecanico(int mecanicoId)
    {
        if (!db.Usuarios.Any(x => x.Id == mecanicoId && x.Rol == "MECANICO"))
            throw new ApiException(404, $"No existe el mecánico {mecanicoId}.");
    }

    private static HashSet<int> AssignedMechanicIds(ServicioWriteDto input)
    {
        var ids = input.MecanicoIds?.ToHashSet() ?? [];
        if (input.MecanicoId.HasValue)
            ids.Add(input.MecanicoId.Value);
        if (ids.Count > 10)
            throw new ApiException(400, "Una orden puede tener como máximo 10 mecánicos asignados.");
        return ids;
    }

    private static HashSet<int> UpdatedMechanicIds(ServicioWriteDto input, Servicio servicio, int? previousLeadId)
    {
        if (input.MecanicoIds is not null)
            return AssignedMechanicIds(input);

        var ids = servicio.MecanicosAsignados.Select(x => x.MecanicoId).ToHashSet();
        if (previousLeadId.HasValue && previousLeadId != input.MecanicoId)
            ids.Remove(previousLeadId.Value);
        if (input.MecanicoId.HasValue)
            ids.Add(input.MecanicoId.Value);
        return ids;
    }

    private void SyncMecanicos(Servicio servicio, HashSet<int> desiredIds)
    {
        foreach (var current in servicio.MecanicosAsignados.Where(x => !desiredIds.Contains(x.MecanicoId)).ToList())
            db.ServiciosMecanicos.Remove(current);

        var currentIds = servicio.MecanicosAsignados.Select(x => x.MecanicoId).ToHashSet();
        foreach (var id in desiredIds.Where(x => !currentIds.Contains(x)))
        {
            servicio.MecanicosAsignados.Add(new ServicioMecanico
            {
                MecanicoId = id,
                FechaAsignacion = DateTimeOffset.UtcNow,
                Servicio = servicio
            });
        }
    }

    private static Servicio Map(ServicioWriteDto input, int? mecanicoId) => new()
    {
        ClienteId = input.ClienteId,
        VehiculoId = input.VehiculoId,
        MecanicoId = mecanicoId,
        Descripcion = input.Descripcion.Trim(),
        TipoMantenimiento = input.TipoMantenimiento.Trim(),
        FechaIngreso = input.FechaIngreso!.Value,
        CostoEstimado = input.CostoEstimado,
        Estado = input.Estado.Trim(),
        FechaCreacion = DateTime.UtcNow
    };

    private static void ApplyChanges(Servicio servicio, ServicioWriteDto input)
    {
        servicio.ClienteId = input.ClienteId;
        servicio.VehiculoId = input.VehiculoId;
        servicio.Descripcion = input.Descripcion.Trim();
        servicio.TipoMantenimiento = input.TipoMantenimiento.Trim();
        servicio.FechaIngreso = input.FechaIngreso!.Value;
        servicio.CostoEstimado = input.CostoEstimado;
        servicio.Estado = input.Estado.Trim();
    }

    private void Save() => db.SaveChanges();
}
