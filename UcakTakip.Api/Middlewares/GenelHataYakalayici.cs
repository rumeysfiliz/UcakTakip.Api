using System.Net;
using System.Text.Json;

namespace UcakTakip.Api.Middlewares;

public class GenelHataYakalayici
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GenelHataYakalayici> _logger;

    public GenelHataYakalayici(RequestDelegate next, ILogger<GenelHataYakalayici> logger)
    {
        _next = next;
        _logger = logger;
    }


    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Yakalan genel hata: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var hata = new
            {
                hataKodu = "sunucu_hatasi",
                mesaj = ex.Message
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(hata));
        }

    }

   
}
public static class GenelHataYakalayiciExtensions
{
    public static IApplicationBuilder UseGenelHataYakalayici(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GenelHataYakalayici>();
    }
}
