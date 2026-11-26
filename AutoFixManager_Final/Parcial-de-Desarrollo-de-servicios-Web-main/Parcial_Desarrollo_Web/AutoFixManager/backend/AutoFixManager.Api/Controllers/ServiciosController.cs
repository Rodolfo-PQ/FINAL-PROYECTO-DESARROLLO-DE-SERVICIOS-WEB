using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/servicios")]
public sealed class ServiciosController(IServicioService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Servicio>> GetAll() => Ok(service.GetAll());

    [HttpGet("cliente/{clienteId:int}")]
    public ActionResult<IReadOnlyList<Servicio>> GetByCliente(int clienteId) => Ok(service.GetByCliente(clienteId));

    [HttpGet("estado/{estado}")]
    public ActionResult<IReadOnlyList<Servicio>> GetByEstado(string estado) => Ok(service.GetByEstado(estado));

    [HttpGet("{id:int}")]
    public ActionResult<Servicio> GetById(int id) => Ok(service.GetById(id));

    [Authorize(Roles = "ADMIN,MECANICO")]
    [HttpPost]
    public ActionResult<Servicio> Create(ServicioWriteDto input)
    {
        var serviceItem = service.Create(input);
        return CreatedAtAction(nameof(GetById), new { id = serviceItem.Id }, serviceItem);
    }

    [Authorize(Roles = "ADMIN,MECANICO")]
    [HttpPut("{id:int}")]
    public ActionResult<Servicio> Update(int id, ServicioWriteDto input) => Ok(service.Update(id, input));

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        service.Delete(id);
        return NoContent();
    }
}
