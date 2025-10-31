using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; //EF Core asenkron DB işlemleri (ToListAsync, AnyAsync, Include, vs.)
using UcakTakip.Api.Data;      // AppDbContext'e erişmek için (DbSet<UcusPlani> burada)
using UcakTakip.Api.Models;    // UcusPlani modeline erişmek için

namespace UcakTakip.Api.Controllers;


[Route("api/[controller]")]
[ApiController]

public class UcusPlaniController : ControllerBase
{
    private readonly AppDbContext _context; //Veri tabanı işlemleri/erişimi için context

    public UcusPlaniController(AppDbContext context)
    {
        _context = context;
    }

   
    // Yardımcılar
    // Gelen/saklanan DateTime'ların UTC olduğundan emin olmak (dönüşüm yap)
    private static DateTime AsUtc(DateTime dt)
    {
        if (dt.Kind == DateTimeKind.Utc) return dt;
        // Unspecified veya Local geldiyse, yerel varsay → UTC'ye çevir
        var local = DateTime.SpecifyKind(dt, DateTimeKind.Local);
        return local.ToUniversalTime();
    }
    //Böylece sistemdeki herkes hangi ülkeden olursa olsun aynı referans zamanı üzerinden çalışabilir.

    private static DateTime? AsUtc(DateTime? dt)
        => dt.HasValue ? AsUtc(dt.Value) : (DateTime?)null;

    //Gelen veriyi düzenli hale getirmek için (Örn: kodu büyük harfe çevir, boşlukları temizle, tarihleri UTC yap). ("Trim boşlukları siliyor. "ToUpperInvariant" büyük harfe çeviriyor. "AsUtc" Utc çeviriyor.)
    private static void Normalize(UcusPlani p)
    {
        p.Code = (p.Code ?? string.Empty).Trim().ToUpperInvariant();
        // Eğer koordinat sistemine geçildiyse, bunlara dokunmaya gerek yok.
        // (İstersen test için tut, ama boş gelebilir.)
        if (!string.IsNullOrWhiteSpace(p.Origin))
            p.Origin = p.Origin.Trim().ToUpperInvariant();

        if (!string.IsNullOrWhiteSpace(p.Destination))
            p.Destination = p.Destination.Trim().ToUpperInvariant();

        p.StartTimeUtc = AsUtc(p.StartTimeUtc);
        p.EndTimeUtc = AsUtc(p.EndTimeUtc);
    }


    //------------------------------------------------------------------------------------------------------------------
    // GET: api/UcusPlani
    // /api/UcusPlani?includePositions=true => konumları da dahil et
    // Tüm uçuş planlarını listelemek için. Frontend bu veriyi alıp haritada gösterir.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UcusPlani>>> GetUcusPlanlari([FromQuery] bool includePositions = false)
    {
        IQueryable<UcusPlani> query = _context.UcusPlanlari.AsNoTracking(); //AsNoTracking: Sadece okuyorum değiştirmeyeceğim demek, performans için önemli.


        if (includePositions)
            query = query.Include(u => u.UcakKonumlari); //UcusPlani ile lişkili UcakKonum kayıtlarını da dahil et. Böylece her planla birlikte o planın tüm konum listesini de getirir. Tek seferde biter.


        var list = await query
        .OrderByDescending(u => u.StartTimeUtc) //En yeni planlar önce gelsin
        .ToListAsync(); //Listeyi al


        return Ok(list); //Listeyi döner

    }

    //------------------------------------------------------------------------------------------------------------------
    // GET: api/UcusPlani/5
    // Belirli bir uçuş planının detayını görmek için (detay sayfası) 
    // İlişkili UcakKonum kayıtlarını da dahil ederiz.

    [HttpGet("{id}")]
    public async Task<ActionResult<UcusPlani>> GetUcusPlani(int id)
    {
        var ucusPlani = await _context.UcusPlanlari
                            .Include(u => u.UcakKonumlari) //uçuş planı ile ilişkili konumları da dahil eDEREK GETİR.
                            .FirstOrDefaultAsync(u => u.Id == id); //Belirli ID'ye sahip uçuş planını getir

        if (ucusPlani == null)
            return NotFound(); //Yoksa 404 döneriz
        return ucusPlani;  //Uçuş planını ve ilişkili konumları döner 
    }


    //------------------------------------------------------------------------------------------------------------------
    // POST: api/UcusPlani
    // Yeni bir uçuş planı eklemek için kullanılır. Frontend'den form ile veri gelir.
    [HttpPost]
    public async Task<ActionResult<UcusPlani>> PostUcusPlani(UcusPlani ucusPlani)
    {
        // Temel validasyon kontrolleri
        if (ucusPlani.EndTimeUtc.HasValue && ucusPlani.EndTimeUtc < ucusPlani.StartTimeUtc)
            return BadRequest("EndTimeUtc, StartTimeUtc'dan küçük olamaz."); //Bitiş zamanı, başlangıçtan küçük olamaz


        if (ucusPlani.OriginLat < -90 || ucusPlani.OriginLat >90)
            return BadRequest("Kalkış enlemi [-90, 90] aralığında olmalı.");
        if (ucusPlani.DestinationLat < -90 || ucusPlani.DestinationLat > 90)
            return BadRequest("Varış enlemi [-90, 90] aralığında olmalı.");
        if (ucusPlani.OriginLat < -180 || ucusPlani.OriginLat > 180)
            return BadRequest("Kalkış boylamı [-180, 180] aralığında olmalı.");
        if (ucusPlani.DestinationLat < -180 || ucusPlani.DestinationLat > 180)
            return BadRequest("Varış boylamı [-180, 180] aralığında olmalı.");

        Normalize(ucusPlani); //Veriyi düzenli hale getir
        ucusPlani.CreatedAtUtc = DateTime.UtcNow; //Kayıt zamanını şu an UTC(dünya saati) yap

        _context.UcusPlanlari.Add(ucusPlani); //Veriyi eklemeye hazırla (henüz eklemiyor bellekte bekliyor!)


        await _context.SaveChangesAsync(); //Değişiklikleri kaydet (SQL'e ekleme işlemi burada gerçekleşir)

        return CreatedAtAction(nameof(GetUcusPlani), new { id = ucusPlani.Id }, ucusPlani); //201 Created döner ve eklenen veriyi geri gönderir 
    }

    //------------------------------------------------------------------------------------------------------------------
    // PUT: api/UcusPlani/5
    // Mevcut bir uçuş planını güncellemek için kullanılır.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUcusPlani(int id, UcusPlani ucusPlani)
    {
        if (id != ucusPlani.Id)
            return BadRequest("Routeden gelen ID ile veri içindeki ID uyuşmuyor."); //ID'ler uyuşmuyorsa 400 Bad Request döner.


        if (ucusPlani.EndTimeUtc.HasValue && ucusPlani.EndTimeUtc < ucusPlani.StartTimeUtc)
            return BadRequest("EndTimeUtc, StartTimeUtc'dan küçük olamaz."); //Bitiş zamanı, başlangıçtan küçük olamaz

        if (ucusPlani.OriginLat < -90 || ucusPlani.OriginLat > 90)
            return BadRequest("Kalkış enlemi [-90, 90] aralığında olmalı.");
        if (ucusPlani.DestinationLat < -90 || ucusPlani.DestinationLat > 90)
            return BadRequest("Varış enlemi [-90, 90] aralığında olmalı.");
        if (ucusPlani.OriginLat < -180 || ucusPlani.OriginLat > 180)
            return BadRequest("Kalkış boylamı [-180, 180] aralığında olmalı.");
        if (ucusPlani.DestinationLat < -180 || ucusPlani.DestinationLat > 180)
            return BadRequest("Varış boylamı [-180, 180] aralığında olmalı.");

        var entity = await _context.UcusPlanlari.FirstOrDefaultAsync(u => u.Id == id); //Veri tabanından mevcut kaydı al
        if (entity == null) return NotFound();

        Normalize(ucusPlani);
        // Mevcut kaydı tek tek set et, güncelle.
        entity.Code = ucusPlani.Code;
        entity.OriginLat = ucusPlani.OriginLat;
        entity.DestinationLat = ucusPlani.DestinationLat;
        entity.StartTimeUtc = ucusPlani.StartTimeUtc;
        entity.EndTimeUtc = ucusPlani.EndTimeUtc;

        if (entity.EndTimeUtc.HasValue && entity.EndTimeUtc < entity.StartTimeUtc)
            return BadRequest("Bitiş zamanı, başlangıç zamanından küçük olamaz.");

        await _context.SaveChangesAsync();
        return NoContent();

    }

    //------------------------------------------------------------------------------------------------------------------
    // DELETE: api/UcusPlani/5
    // Kayıt silme işlemi için örnek iptal edilen plan
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUcusPlani(int id)
    {
        //İlk önce uçuş planı var mı kontrol et
        var ucusPlani = await _context.UcusPlanlari.FindAsync(id);
        if (ucusPlani == null)
            return NotFound(); //Yoksa 404 döneriz

        _context.UcusPlanlari.Remove(ucusPlani); //Uçuş planını sil
        await _context.SaveChangesAsync(); //Değişiklikleri kaydet
        return NoContent(); //Başarılı silme sonrası 204 No Content döneriz
    }


    //-----------------------------------------------------------------------------------------------------------------
    //GET: api/UcusPlani/tarih?baslangıc=2025-10-01T00:00:00Z&bitis=2025-10-31T23:59:59Z
    // Belirli bir tarih aralığındaki uçuş planlarını getirmek için 

    [HttpGet("tarih")]
    public async Task<ActionResult<IEnumerable<UcusPlani>>> TarihAraligi(
        [FromQuery] DateTime baslangic,
        [FromQuery] DateTime bitis,
        [FromQuery] bool includePositions = false) //Konumları da dahil etmek için opsiyon
    {
        var b1 = AsUtc(baslangic); //Kullanıcı hangi saat diliminden gönderirse göndersin, UTC’ye çeviriyoruz. Böylece “yaz/kış saati, ülke farkı” gibi karışıklıklar olmaz.
        var b2 = AsUtc(bitis);
        if (b2 < b1) return BadRequest("Bitiş tarihi başlangıçtan küçük olamaz.");

        IQueryable<UcusPlani> up = _context.UcusPlanlari.AsNoTracking()
            .Where(u => u.StartTimeUtc >= b1 && u.StartTimeUtc <= b2);

        if (includePositions)
            up = up.Include(u => u.UcakKonumlari);

        var data = await up.ToListAsync();
        return data;
    }
}