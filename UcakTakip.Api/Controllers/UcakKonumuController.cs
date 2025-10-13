using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UcakTakip.Api.Data;
using UcakTakip.Api.Models;
using System.Linq;
using UcakTakip.Api.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace UcakTakip.Api.Controllers;

[Route("api/[controller]")] //controller'a gelen isteklerin adresini belirler.
[ApiController] //controller'ın bir API controller olduğunu belirtir.
public class UcakKonumuController : ControllerBase
{
    private readonly AppDbContext _context; //Veritabanı işlemleri/erişimi için context

    
    //AppDbContext'i otomatik olarak dependency injection (bağımlılık) yoluyla _context'e atayarak her metotta veri tabanına ulaşabiliriz.
    //!!Yani _context aslında veri tabanına erişmemizi sağlayan nesnedir. Artık SQL sorgusu yazmadan, kodla tablo işlemleri yapabileceğiz.
    public UcakKonumuController(AppDbContext context)
    {
        _context = context; 
    }


    //Bu metot tüm üçak konumlarını getir demek. GET: api/UcakKonumu
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UcakKonum>>> GetUcakKonumlari(
        [FromQuery] int page = 1, //sayfa numarası
        [FromQuery] int pageSize = 100) //sayfa başına kayıt sayısı)
    {
        page = page < 1 ? 1 : page; //sayfa 1'den küçük olamaz
        pageSize = pageSize is > 1000 or < 1 ? 100 : pageSize; //sayfa başına kayıt sayısı 1-1000 arasında olmalı

        //Sayfalama (pagination) için Skip ve Take kullanıyoruz. 
        //Kayıtları zaman sırasına göre getiriyoruz.
        //Böylece en eski kayıtlar önce gelir
        var q = _context.UcakKonumlari.OrderBy(x => x.TimestampUtc);
        var data = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(); //sayfalama işlemi
        return data; //veri tabanından gelen tüm uçak konum kayıtlarını döner. 

    }


    //Bu metot ID'ye göre tek bir kaydın konum bilgisini getirir. GET: api/UcakKonumu/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UcakKonum>> GetUcakKonum(int id)
    {
        var ucakKonum = await _context.UcakKonumlari.FindAsync(id);
        if (ucakKonum == null)
        {
            return NotFound();
        }
        return ucakKonum;
    }

    //Bu metot belirli bir uçuş planına (ucusPlaniId) ait en son konum kaydını getirir. GET: api/UcakKonumu/son-konum/3
    [HttpGet("son-konum/{ucusPlaniId:int}")]
    public async Task<ActionResult<UcakKonum>> SonKonum(int ucusPlaniId)
    {
        //Belirli bir uçuş planına (ucusPlaniId) ait en son konum kaydını getirir.
        var sonKonum = await _context.UcakKonumlari
            .Where(uk=>uk.UcusPlaniId == ucusPlaniId)
            .OrderByDescending(uk => uk.TimestampUtc)
            .FirstOrDefaultAsync();

        return sonKonum is null ? NotFound() : sonKonum; //Kayıt yoksa 404 döner, varsa konum bilgisini döner.
    }


    //Bu metot belirli bir zaman aralığında (fromUtc, toUtc) ve belirli bir uçuş planına (ucusPlaniId) ait konum kayıtlarını getirir. GET: api/UcakKonumu/aralik?ucusPlaniId=3&fromUtc
    //Sayfalama için page ve pageSize parametreleri de ekledik.
    [HttpGet("aralik")]
    public async Task<ActionResult<IEnumerable<UcakKonum>>> Aralik(
        [FromQuery] int ucusPlaniId,
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,    
        [FromQuery] int page =1,
        [FromQuery] int pageSize = 1000)
    {
        if (fromUtc > toUtc) return BadRequest("fromUtc, toUtc'dan büyük olamaz.");

        var q = _context.UcakKonumlari
            .Where(uk=> uk.UcusPlaniId ==ucusPlaniId && uk.TimestampUtc >= fromUtc && uk.TimestampUtc <= toUtc)
            .OrderBy(uk=> uk.TimestampUtc); //Zaman sırasına göre sırala

        var data = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(); //sayfalama işlemi
        return data;

    }


    //Bu metot Frontend'den form gönderildiğinde (yeni bir uçak konum kaydı ekler.) POST: api/UcakKonumu
    [HttpPost, ApiKey]
    public async Task<ActionResult<UcakKonum>> PostUcakKonumu(UcakKonum konum)
    {
        if(!ModelState.IsValid)
            return ValidationProblem(ModelState); //Model doğrulaması başarısız ise 400 Bad Request döner.

        _context.UcakKonumlari.Add(konum);
        await _context.SaveChangesAsync();  //SQL'e ekleme işlemi burada gerçekleşir.

        return CreatedAtAction(nameof(GetUcakKonum), new {id = konum.Id }, konum);  //ekleme başarılı ise 201 Created döner ve eklenen veriyi geri gönderir.
    }

    //Uçuşun konum bilgisini değiştirmek veya düzeltmek istersek PUT: api/UcakKonumu/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUcakKonumu(int id, UcakKonum konum)
    {
        if (id != konum.Id)
            return BadRequest(); //ID'ler uyuşmuyorsa 400 Bad Request döner.

        _context.Entry(konum).State = EntityState.Modified;  //Veriyi güncelleme moduna alır.

        try
        {
            await _context.SaveChangesAsync(); //hata yoksa SQL'e güncelleme işlemi burada gerçekleşir.

        }
        catch (DbUpdateConcurrencyException) 
        { 
            if (!_context.UcakKonumlari.Any(e=> e.Id == id)) 
                return NotFound(); //Eğer kayıt yoksa 404 Not Found döner.
            else
                throw; //Başka bir hata varsa hatayı fırlatır.
        }
        return NoContent();

    }



    //Bir uçak konum kaydını silmek istersek DELETE: api/UcakKonumu/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUcakKonumu(int id)
    {
        var konum = await _context.UcakKonumlari.FindAsync(id);
        if(konum==null)
            return NotFound(); //Kayıt yoksa 404 Not Found döner.

        _context.UcakKonumlari.Remove(konum);
        await _context.SaveChangesAsync(); //SQL'den silme işlemi burada gerçekleşir.
        return NoContent(); //Başarılı silme işleminden sonra 204 No Content döner.

    }


}
