using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Models;

namespace AutoFixManager.Api.Interfaces;

public interface IClienteService
{
    IReadOnlyList<Cliente> GetAll();
    Cliente GetById(int id);
    Cliente Create(ClienteWriteDto input);
    Cliente Update(int id, ClienteWriteDto input);
    void Delete(int id);
}
