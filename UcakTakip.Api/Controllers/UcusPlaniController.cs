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

    // GET: api/UcusPlani
    // Tüm uçuş planlarını listelemek için. Frontend bu veriyi alıp haritada gösterir.
    // UcusPlani ile ilişkili UcakKonum kayıtlarını da dahil ederiz. Böylece tek seferde tüm uçuş ve konum verisini alırız. 
    // Bu, performans açısından daha iyidir çünkü her uçuş için ayrı ayrı konum sorgusu yapmamıza gerek kalmaz.
    //  Include(u => u.UcakKonumlari) ifadesi, EF Core'a UcusPlani ile ilişkili UcakKonum kayıtlarını da yüklemesini söyler. 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UcusPlani>>> GetUcusPlanlari()
    {
        var list = await _context.UcusPlanlari
                        .Include(u => u.UcakKonumlari) //İlişkili UcakKonum kayıtlarını da getir
            .ToListAsync();
        return list;
    }


    // GET: api/UcusPlani/5
    // Belirli bir uçuş planının detayını görmek için (detay sayfası) 
    // İlişkili UcakKonum kayıtlarını da dahil ederiz.
    [HttpGet("{id}")]
    public async Task<ActionResult<UcusPlani>> GetUcusPlani(int id)
    {
        var ucusPlani = await _context.UcusPlanlari
                            .Include(u => u.UcakKonumlari) //İlişkili UcakKonum kayıtlarını da getir
                            .FirstOrDefaultAsync(u => u.Id == id); //Belirli ID'ye sahip uçuş planını getir

        if (ucusPlani == null)
            return NotFound();
        return ucusPlani;  //Uçuş planını ve ilişkili konumları döner 
    }


    // POST: api/UcusPlani
    // Yeni bir uçuş planı eklemek için kullanılır. Frontend'den form ile veri gelir.
    [HttpPost]
    public async Task<ActionResult<UcusPlani>> PostUcusPlani(UcusPlani ucusPlani)
    {
        // Temel validasyon kontrolleri
        if (ucusPlani.EndTimeUtc.HasValue && ucusPlani.EndTimeUtc < ucusPlani.StartTimeUtc)
            return BadRequest("EndTimeUtc, StartTimeUtc'dan küçük olamaz."); //Bitiş zamanı, başlangıçtan küçük olamaz

        if (string.IsNullOrWhiteSpace(ucusPlani.Origin) || string.IsNullOrWhiteSpace(ucusPlani.Destination))
            return BadRequest("Origin ve Destination boş olamaz."); //Kalkış ve varış boş olamaz

        // Aynı Code ve StartTimeUtc'ya sahip bir uçuş planı zaten var mı kontrol et

        _context.UcusPlanlari.Add(ucusPlani); //Yeni uçuş planını ekle


        await _context.SaveChangesAsync(); //Değişiklikleri kaydet (SQL'e ekleme işlemi burada gerçekleşir)

        return CreatedAtAction(nameof(GetUcusPlani), new { id = ucusPlani.Id }, ucusPlani); //201 Created döner ve eklenen veriyi geri gönderir 
    }

    // PUT: api/UcusPlani/5
    // Mevcut bir uçuş planını güncellemek için kullanılır.
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUcusPlani(int id , UcusPlani ucusPlani)
    {
        if (id !=ucusPlani.Id)
            return BadRequest("Routeden gelen ID ile veri içindeki ID uyuşmuyor."); //ID'ler uyuşmuyorsa 400 Bad Request döner.
        

        if (ucusPlani.EndTimeUtc.HasValue && ucusPlani.EndTimeUtc < ucusPlani.StartTimeUtc)
            return BadRequest("EndTimeUtc, StartTimeUtc'dan küçük olamaz."); //Bitiş zamanı, başlangıçtan küçük olamaz

        if (string.IsNullOrWhiteSpace(ucusPlani.Origin) || string.IsNullOrWhiteSpace(ucusPlani.Destination))
            return BadRequest("Origin ve Destination boş olamaz."); //Kalkış ve varış boş olamaz


        //!!!!!!!!DÜZENLİCEZ BURAYI !!!!!!!!!!

        //Detached -> yani EF bu nesnenin önceden veri tabanında olduğunu bilmiyor.
        //Bu ucusPlani nesnesi veri tabanından gelmedi, frontend'den gelen yeni bir kopya.
        //modified yaparsak EF bunu güncelleme olarak algılar ve tüm alanları günceller. yani ef önce bul sonra güncelle adımlarını atlıyor.
        _context.Entry(ucusPlani).State = EntityState.Modified; //Veriyi güncelleme moduna al

        try
        {
            await _context.SaveChangesAsync(); //Update db
        }
        catch(DbUpdateConcurrencyException)         {
            //Eğer güncelleme sırasında veri bulunamazsa (başka bir yerde silinmiş olabilir) 404 döneriz.
            var exists = await _context.UcusPlanlari.AnyAsync(u => u.Id == id); 
            if (!exists)
                return NotFound();
            else
                throw; //Başka bir hata varsa hatayı fırlat

        }
        return NoContent(); //Başarılı güncelleme sonrası 204 No Content döneriz

    }


    // DELETE: api/UcusPlani/5
    // Kayıt silme işlemi için örnek iptal edilen plan
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUcusPlani(int id)
    {
        //İlk önce uçuş planı var mı kontrol et
        var ucusPlani = await _context.UcusPlanlari.FindAsync(id);
        if(ucusPlani == null) 
            return NotFound(); //Yoksa 404 döneriz

        _context.UcusPlanlari.Remove(ucusPlani); //Uçuş planını sil
        await _context.SaveChangesAsync(); //Değişiklikleri kaydet
        return NoContent(); //Başarılı silme sonrası 204 No Content döneriz
    }


    //----------------------------------------------------------------
    //----------------------------------------------------------------

    //Get: api/UcusPlani(ara?origin=IST&destination=*
    // Uçuş planlarını kalkış ve varışa göre aramak için
    [HttpGet("ara")]
    public async Task<ActionResult<IEnumerable<UcusPlani>>> Ara(
        [FromQuery] string? origin,
        [FromQuery] string? destination,
        [FromQuery] bool includePositions =false) //Konumları da dahil etme opsiyonu
    {
        IQueryable<UcusPlani> up = _context.UcusPlanlari;

        //ToLower ile büyük küçük harf duyarsız arama yapıyoruz. 

        if (!string.IsNullOrWhiteSpace(origin))
            up = up.Where(u => u.Origin != null && u.Origin.ToLower() == origin.ToLower()); //Kalkış yeri filtrele
        if (!string.IsNullOrWhiteSpace(destination))
            up = up.Where(u => u.Destination != null && u.Destination.ToLower() == destination.ToLower()); //Varış yeri filtrele
        if (includePositions) 
            up = up.Include(u => u.UcakKonumlari); //İlişkili UcakKonum kayıtlarını da getir
        var data = await up.ToListAsync();
        return data;

    }



    //GET: api/UcusPlani/tarih?baslangıc=2025-10-01T00:00:00Z&bitis=2025-10-31T23:59:59Z
    // Belirli bir tarih aralığındaki uçuş planlarını getirmek için 

    [HttpGet("tarih")]
    public async Task<ActionResult<IEnumerable<UcusPlani>>> TarihAraligi(
        [FromQuery] DateTime baslangic,
        [FromQuery] DateTime bitis,
        [FromQuery] bool includePositions = false) //Konumları da dahil etme opsiyonu)
    {
        if (bitis < baslangic)
            return BadRequest("Bitiş tarihi başlangıçtan küçük olamaz.");

        IQueryable<UcusPlani> up = _context.UcusPlanlari.Where(u => u.StartTimeUtc >= baslangic && u.StartTimeUtc <= bitis); //Tarih aralığını filtrele

        if(includePositions)
            up = up.Include(u => u.UcakKonumlari); //İlişkili UcakKonum kayıtlarını da getir

        var data = await up.ToListAsync();
        return data;
    }
}
