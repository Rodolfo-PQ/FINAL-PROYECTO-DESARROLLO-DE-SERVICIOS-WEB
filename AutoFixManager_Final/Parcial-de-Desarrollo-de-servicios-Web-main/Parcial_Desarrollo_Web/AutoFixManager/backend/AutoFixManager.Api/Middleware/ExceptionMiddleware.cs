using AutoFixManager.Api.Middleware;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;

namespace AutoFixManager.Api.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException exception)
        {
            await WriteError(context, exception.StatusCode, exception.Message);
        }
        catch (DbUpdateException exception)
        {
            var databaseException = exception.GetBaseException() as MySqlException;
            var (statusCode, message) = databaseException?.Number switch
            {
                1062 => (StatusCodes.Status409Conflict, "El registro ya existe."),
                1451 or 1452 => (StatusCodes.Status409Conflict, "La operación contradice una relación o restricción de datos."),
                1048 => (StatusCodes.Status400BadRequest, "Falta un valor requerido por la base de datos."),
                1264 => (StatusCodes.Status400BadRequest, "Un valor excede el rango permitido."),
                1205 or 1213 => (StatusCodes.Status503ServiceUnavailable, "La operación tuvo un conflicto temporal; intenta de nuevo."),
                _ => (StatusCodes.Status500InternalServerError, "No fue posible guardar los datos.")
            };
            LogDatabaseException(exception, statusCode);
            await WriteError(context, statusCode, message);
        }
        catch (MySqlException exception)
        {
            var statusCode = exception.Number is 1042 or 2002 or 2003 or 2006 or 2013
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status500InternalServerError;
            LogDatabaseException(exception, statusCode);
            await WriteError(context, statusCode,
                statusCode == StatusCodes.Status503ServiceUnavailable
                    ? "La base de datos no está disponible temporalmente."
                    : "Ocurrió un error interno.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API error");
            await WriteError(context, StatusCodes.Status500InternalServerError, "Ocurrió un error interno.");
        }
    }

    private void LogDatabaseException(Exception exception, int statusCode)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Database operation failed with status {StatusCode}", statusCode);
        else
            logger.LogWarning(exception, "Database constraint rejected the request with status {StatusCode}", statusCode);
    }

    private static async Task WriteError(HttpContext context, int statusCode, string message)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { status = statusCode, error = message });
    }
}
