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
    [Required]
    public DateTime StartTimeUtc { get; set; }
    public DateTime? EndTimeUtc { get; set; } //bilinmiyorsa null olabilir

    [Required]
    public string Origin { get; set; } = string.Empty; //Kalkış yeri
    [Required]
    public string Destination { get; set; } = string.Empty; //Varış yeri

    //Kayıt zaman bilgisi (log ve sıralama için
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    //1Uçuş -> N UcakKonum (uçağın zaman içindeki konumunu izleriz.)
    public ICollection<UcakKonum> UcakKonumlari { get; set; } = new List<UcakKonum>();


}
