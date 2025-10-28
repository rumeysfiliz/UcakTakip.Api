using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using System.Runtime.Intrinsics.X86;


namespace UcakTakip.Api.Filters;


//Sadece bizim belirlediğimiz gizli anahtarı (API Key) bilen kişi veri gönderebilmesi için yaptık.  Örnek: [HttpPost, ApiKey]
//Sadece GET yapan endpointler herkese açık olabilir (okuma serbest). Ama ekleme, silme, güncelleme yapan(POST, PUT, DELETE) endpointler, ancak doğru API anahtarına sahip olan kişi tarafından kullanılabilir.

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)] 
public class ApiKeyAttribute :Attribute, IAsyncActionFilter 
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) 
    {
        // API anahtarını doğrulama işlemi 
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>(); 
        var expected = config["Simulator:ApiKey"]; // appsettings.json içindeki anahtar 

        //Hiç tanımlı değilse 500 döner.çünkü bu bir sunucu hatasıdır.
        if (String.IsNullOrWhiteSpace(expected))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status500InternalServerError); // Sunucu hatası
            return; 
        }

        //gelen istek headerında X-API-KEY var mı, doğru mu kontrol.  Header’dan "X-API-KEY" bilgisini al
        if (!context.HttpContext.Request.Headers.TryGetValue("X-API-Key", out var provided) || provided != expected)
        {
            context.Result = new UnauthorizedResult(); // 401 Unauthorized
            return;
        }

        await next(); // Doğrulama başarılı ise işlemi devam ettir
    }
}
