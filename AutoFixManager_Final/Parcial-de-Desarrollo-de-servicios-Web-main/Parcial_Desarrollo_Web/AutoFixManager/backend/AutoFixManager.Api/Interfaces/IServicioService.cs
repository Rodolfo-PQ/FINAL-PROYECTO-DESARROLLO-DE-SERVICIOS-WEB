using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Models;

namespace AutoFixManager.Api.Interfaces;

public interface IServicioService
{
    IReadOnlyList<Servicio> GetAll();
    IReadOnlyList<Servicio> GetByCliente(int clienteId);
    IReadOnlyList<Servicio> GetByMecanico(int mecanicoId);
    IReadOnlyList<Cliente> GetClientesByMecanico(int mecanicoId);
    IReadOnlyList<Vehiculo> GetVehiculosByMecanico(int mecanicoId);
    IReadOnlyList<Servicio> GetByEstado(string estado);
    Servicio GetById(int id);
    Servicio GetByIdForMecanico(int id, int mecanicoId);
    bool IsAssignedToMecanico(int servicioId, int mecanicoId);
    Servicio Create(ServicioWriteDto input);
    Servicio CreateForMecanico(ServicioWriteDto input, int mecanicoId);
    Servicio Update(int id, ServicioWriteDto input);
    Servicio UpdateForMecanico(int id, ServicioWriteDto input, int mecanicoId);
    void Delete(int id);
}
