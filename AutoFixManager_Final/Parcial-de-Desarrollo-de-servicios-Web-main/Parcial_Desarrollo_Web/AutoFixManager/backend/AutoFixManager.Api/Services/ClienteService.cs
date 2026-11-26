using AutoFixManager.Api.Data; using AutoFixManager.Api.DTOs; using AutoFixManager.Api.Interfaces; using AutoFixManager.Api.Middleware; using AutoFixManager.Api.Models; using Microsoft.EntityFrameworkCore; using AutoFixManager.Api.Repositories;
namespace AutoFixManager.Api.Services;
public sealed class ClienteService(AutoFixDbContext db, IRepository<Cliente> repository) : IClienteService
{
 public IReadOnlyList<Cliente> GetAll() => repository.Query().AsNoTracking().OrderBy(x=>x.Id).ToList();
 public Cliente GetById(int id) => repository.Query().AsNoTracking().FirstOrDefault(x=>x.Id==id) ?? throw new ApiException(404,$"No existe el cliente {id}.");
 public Cliente Create(ClienteWriteDto input) { var email=input.Email.Trim(); if(db.Clientes.Any(x=>x.Email.ToLower()==email.ToLower())) throw new ApiException(409,"Ya existe un cliente con ese correo electrónico."); var c=new Cliente { Nombre=input.Nombre.Trim(), Apellido=input.Apellido.Trim(), Email=email, Telefono=input.Telefono.Trim(), FechaRegistro=DateTime.UtcNow }; repository.Add(c); Save(); return c; }
 public Cliente Update(int id, ClienteWriteDto input) { var c=repository.Find(id)??throw new ApiException(404,$"No existe el cliente {id}."); var email=input.Email.Trim(); if(db.Clientes.Any(x=>x.Id!=id && x.Email.ToLower()==email.ToLower())) throw new ApiException(409,"Ya existe un cliente con ese correo electrónico."); c.Nombre=input.Nombre.Trim(); c.Apellido=input.Apellido.Trim(); c.Email=email; c.Telefono=input.Telefono.Trim(); Save(); return c; }
 public void Delete(int id) { var c=db.Clientes.Find(id)??throw new ApiException(404,$"No existe el cliente {id}."); if(db.Vehiculos.Any(x=>x.ClienteId==id)||db.Servicios.Any(x=>x.ClienteId==id)||db.Usuarios.Any(x=>x.ClienteId==id)) throw new ApiException(409,"No se puede eliminar el cliente porque tiene vehículos, servicios o una cuenta relacionada."); repository.Remove(c); Save(); }
 private void Save()=>repository.Save();
}
