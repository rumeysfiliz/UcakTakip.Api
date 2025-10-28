using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace UcakTakip.Api.Models;

//Sistemde planlanan tek bir uçuşu temsil eder.
public class UcusPlani
{
    // Primary key : Her uçuşta DB otomatik benzersiz ID atar
    public int Id { get; set; }


    //Uçak kodu (örneğin "THY203")
    [Required, StringLength(15)]
    public string Code { get; set; } = string.Empty;


    // Kalkış ve varış bilgisi (UTC saklarız çünkü zaman hatası yaşamayalım
    private DateTime _startTimeUtc;
    [Required]
    public DateTime StartTimeUtc
    {
        get => DateTime.SpecifyKind(_startTimeUtc, DateTimeKind.Utc);
        set => _startTimeUtc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private DateTime? _endTimeUtc;
    public DateTime? EndTimeUtc
    {
        get => _endTimeUtc.HasValue ? DateTime.SpecifyKind(_endTimeUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
        set => _endTimeUtc = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : (DateTime?)null;
    }

    [Required]
    public string Origin { get; set; } = string.Empty; //Kalkış yeri
    [Required]
    public string Destination { get; set; } = string.Empty; //Varış yeri

    //Kayıt zaman bilgisi (log ve sıralama için
    private DateTime _createdAtUtc = DateTime.UtcNow;
    public DateTime CreatedAtUtc
    {
        get => DateTime.SpecifyKind(_createdAtUtc, DateTimeKind.Utc);
        set => _createdAtUtc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    //1Uçuş -> N UcakKonum (uçağın zaman içindeki konumunu izleriz.)
    public ICollection<UcakKonum> UcakKonumlari { get; set; } = new List<UcakKonum>();


}
