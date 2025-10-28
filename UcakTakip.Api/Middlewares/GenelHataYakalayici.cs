using System.Net;
using System.Text.Json;

namespace UcakTakip.Api.Middlewares;


//Middleware => Ara Katman. Yani istek(request) ile cevap(response) arasına giren bir ara parça
//Amaç: Herhangi bir yerde beklenmeyen hata fırlatırsa, uygulamanın çökmesini önlemek ve kullanıcıya anlamlı hata mesajları sunmak.
//Gelen istek önce buradan geçiyor. Eğer arkada (controller'da) bir hata olursa, bu middleware o hatayı yakalıyor ve düzgün bir JSON döndürüyor.
public class GenelHataYakalayici
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GenelHataYakalayici> _logger;

    public GenelHataYakalayici(RequestDelegate next, ILogger<GenelHataYakalayici> logger)
    {
        _next = next; //siradaki middleware/endpointe devam etmek için temsilci
        _logger = logger; //hata olursa günlük(log) atmak için
    }


    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context); //ilk önce istek normal akışta ilerliyor.
        }
        catch (Exception ex) //hiç yakalnmamış bir istisna varsa buraya düşüyor.
        {
            _logger.LogError(ex, "Yakalan genel hata: {Message}", ex.Message); //mesajı loga yazıyor.
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; //genel sunucu hatası
            context.Response.ContentType = "application/json"; //Frontende düzgün JSON gider.

            var hata = new //dönen gövde
            {
                hataKodu = "sunucu_hatasi",
                mesaj = ex.Message
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(hata));
        }
    }
}
public static class GenelHataYakalayiciExtensions //app.UseGenelHataYakalayici(); --> Program.cs eklenti
{
    public static IApplicationBuilder UseGenelHataYakalayici(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GenelHataYakalayici>();
    }
}
