using System.Security.Claims;
using AutoFixManager.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutoFixManager.Api.Filters;

public sealed class MecanicoAsignacionFilter(IServicioService servicioService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdValue = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var serviceIdValue = context.RouteData.Values["id"]?.ToString();
        if (!int.TryParse(userIdValue, out var userId) || !int.TryParse(serviceIdValue, out var serviceId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!servicioService.IsAssignedToMecanico(serviceId, userId))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class RequireMecanicoAsignacionAttribute : TypeFilterAttribute
{
    public RequireMecanicoAsignacionAttribute() : base(typeof(MecanicoAsignacionFilter)) { }
}
