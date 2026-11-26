using AutoFixManager.Api.DTOs;
using AutoFixManager.Api.Interfaces;
using AutoFixManager.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AutoFixManager.Api.Controllers;

[ApiController]
[Authorize(Roles = "ADMIN")]
[Route("api/vehiculos")]
public sealed class VehiculosController(IVehiculoService service) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<Vehiculo>> GetAll() => Ok(service.GetAll());

    [HttpGet("cliente/{clienteId:int}")]
    public ActionResult<IReadOnlyList<Vehiculo>> GetByCliente(int clienteId) => Ok(service.GetByCliente(clienteId));

    [HttpGet("{id:int}")]
    public ActionResult<Vehiculo> GetById(int id) => Ok(service.GetById(id));

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public ActionResult<Vehiculo> Create(VehiculoWriteDto input)
    {
        var vehicle = service.Create(input);
        return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id:int}")]
    public ActionResult<Vehiculo> Update(int id, VehiculoWriteDto input) => Ok(service.Update(id, input));

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        service.Delete(id);
        return NoContent();
    }
}
