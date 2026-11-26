using AutoFixManager.Api.Data; using AutoFixManager.Api.DTOs; using AutoFixManager.Api.Interfaces; using AutoFixManager.Api.Middleware; using AutoFixManager.Api.Models; using Microsoft.EntityFrameworkCore;
namespace AutoFixManager.Api.Services;
public sealed class VehiculoService(AutoFixDbContext db) : IVehiculoService
{
 public IReadOnlyList<Vehiculo> GetAll()=>db.Vehiculos.AsNoTracking().OrderBy(x=>x.Id).ToList();
 public IReadOnlyList<Vehiculo> GetByCliente(int clienteId){if(!db.Clientes.Any(x=>x.Id==clienteId))throw new ApiException(404,$"No existe el cliente {clienteId}.");return db.Vehiculos.AsNoTracking().Where(x=>x.ClienteId==clienteId).ToList();}
 public Vehiculo GetById(int id)=>db.Vehiculos.AsNoTracking().FirstOrDefault(x=>x.Id==id)??throw new ApiException(404,$"No existe el vehículo {id}.");
 public Vehiculo Create(VehiculoWriteDto input){Validate(input,0);var v=Map(input);db.Vehiculos.Add(v);Save();return v;}
 public Vehiculo Update(int id,VehiculoWriteDto input){var v=db.Vehiculos.Find(id)??throw new ApiException(404,$"No existe el vehículo {id}.");Validate(input,id);v.ClienteId=input.ClienteId;v.Placa=input.Placa.Trim().ToUpperInvariant();v.Marca=input.Marca.Trim();v.Modelo=input.Modelo.Trim();v.Anio=input.Anio;v.Color=input.Color.Trim();Save();return v;}
 public void Delete(int id){var v=db.Vehiculos.Find(id)??throw new ApiException(404,$"No existe el vehículo {id}.");if(db.Servicios.Any(x=>x.VehiculoId==id))throw new ApiException(409,"No se puede eliminar el vehículo porque tiene servicios relacionados.");db.Vehiculos.Remove(v);Save();}
 private void Validate(VehiculoWriteDto x,int ignored){if(!db.Clientes.Any(c=>c.Id==x.ClienteId))throw new ApiException(404,$"No existe el cliente {x.ClienteId}.");if(db.Vehiculos.Any(v=>v.Id!=ignored&&v.Placa.ToLower()==x.Placa.Trim().ToLower()))throw new ApiException(409,"Ya existe un vehículo con esa placa.");}
 private static Vehiculo Map(VehiculoWriteDto x)=>new(){ClienteId=x.ClienteId,Placa=x.Placa.Trim().ToUpperInvariant(),Marca=x.Marca.Trim(),Modelo=x.Modelo.Trim(),Anio=x.Anio,Color=x.Color.Trim()};
 private void Save()=>db.SaveChanges();
}
