using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Models;

namespace AutoFixManager.Api.Interfaces;

public interface IVehiculoService
{
    IReadOnlyList<Vehiculo> GetAll();
    IReadOnlyList<Vehiculo> GetByCliente(int clienteId);
    Vehiculo GetById(int id);
    Vehiculo Create(VehiculoWriteDto input);
    Vehiculo Update(int id, VehiculoWriteDto input);
    void Delete(int id);
}
