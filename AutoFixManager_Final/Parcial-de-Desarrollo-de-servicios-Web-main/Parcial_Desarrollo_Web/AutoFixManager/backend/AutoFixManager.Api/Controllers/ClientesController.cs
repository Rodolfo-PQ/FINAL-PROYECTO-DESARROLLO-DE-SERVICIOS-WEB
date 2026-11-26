using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/clientes")]
public sealed class ClientesController(IClienteService service) : ControllerBase
{

    [HttpGet]
    public ActionResult<IReadOnlyList<Cliente>> GetAll() => Ok(service.GetAll());

    [HttpGet("{id:int}")]
    public ActionResult<Cliente> GetById(int id) => Ok(service.GetById(id));

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public ActionResult<Cliente> Create(ClienteWriteDto input)
    {
        var cliente = service.Create(input);
        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public ActionResult<Cliente> Update(int id, ClienteWriteDto input) => Ok(service.Update(id, input));

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        service.Delete(id);
        return NoContent();
    }
}
