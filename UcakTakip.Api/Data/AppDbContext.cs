using Microsoft.EntityFrameworkCore;
using System;
using UcakTakip.Api.Models;

namespace UcakTakip.Api.Data;


//EF Core’a “kendi context’imi tanımlıyorum” diyoruz. Bu sınıf içinde:
//Hangi tablolara sahip olduğumuzu(DbSet<>)
//Hangi kuralları(index, ilişki) istediğimizi(OnModelCreating) söylüyoruz.

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // UcusPlani ve UcakKonum DbSet'leri
    public DbSet<Models.UcusPlani> UcusPlanlari { get; set; }
    public DbSet<Models.UcakKonum> UcakKonumlari { get; set; }


    // Model yapılandırması performans ve veri bütünlüğü için önemli
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
