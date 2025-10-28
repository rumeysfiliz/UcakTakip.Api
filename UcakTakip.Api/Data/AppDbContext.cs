using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using UcakTakip.Api.Models;

namespace UcakTakip.Api.Data;


//Backend ile veri tabanı arasındaki köprü. Kod yazıyoruz, ama arkada Sbir Veri tabanı var. Normalde "INSERT,INTO.." gibi SQL yazmamız lazım ama EF Core sistemi sayesinde gerek kalmıyor. EF Core, benim C# sınıflarımı alıyor ve diyor ki: "bunlar aslında veri tabanındaki tabloları temsil ediyor."

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // UcusPlani ve UcakKonum DbSet'leri
    public DbSet<Models.UcusPlani> UcusPlanlari { get; set; }
    public DbSet<Models.UcakKonum> UcakKonumlari { get; set; }


    // Model yapılandırması performans ve veri bütünlüğü için önemli
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tüm DateTime alanlarını UTC olarak normalleştir
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            // Save: her şeyi UTC kaydet
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Local).ToUniversalTime(),
            // Read: DB'den geleni UTC olarak işaretle
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var utcNullableConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue
                    ? (v.Value.Kind == DateTimeKind.Utc ? v.Value
                        : DateTime.SpecifyKind(v.Value, DateTimeKind.Local).ToUniversalTime())
                    : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
        );

        modelBuilder.Entity<UcusPlani>().HasIndex(u => u.Code); // Uçak kodu üzerinde indeks

       modelBuilder.Entity<UcusPlani>()
            .Property(u => u.Code).HasMaxLength(15); // Code alanı maksimum 15 karakter



        // UcakKonum için UcusPlaniId ve TimestampUtc  ilr sorgulama birleşik indeksi
        modelBuilder.Entity<UcakKonum>().HasIndex(uk => new {uk.UcusPlaniId, uk.TimestampUtc}); 

        modelBuilder.Entity<UcakKonum>()
            .HasOne(uk => uk.UcusPlani) // UcakKonum bir UcusPlani'ye ait
            .WithMany(up => up.UcakKonumlari) // UcusPlani'nın birden çok UcakKonum'u olabilir
            .HasForeignKey(uk => uk.UcusPlaniId) // Yabancı anahtar 
            .OnDelete(DeleteBehavior.Cascade); // UcusPlani silindiğinde ilişkili UcakKonum'lar da silinsin


        base.OnModelCreating(modelBuilder);


    }

}

//Migrations özelliğiyle de model sınıflarımdan veri tabanı tablolarını otomatik oluşturuyor
//dotnet ef migrations add ilk  --> sınıflara bakar ve gerekli tablo yapısını çıkarır.
//dotnet ef database update     --> Veri tabanında o tabloları oluşturur.
