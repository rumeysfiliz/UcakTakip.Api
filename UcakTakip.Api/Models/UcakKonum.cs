using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;          



namespace UcakTakip.Api.Models;

//Belirli zamanda uçağın konum/yön/irtifa bilgisi
public class UcakKonum
{

    //Çok sayıda kayıt olacağı için PK'yi long seçtik daha güvenli.
    [Key]
    public long Id { get; set; }


    [Required]
    public int UcusPlaniId { get; set; } //Hangi uçuşa ait olduğunu bilmemiz lazım FK


    //Navigasyonel özellik: EF Core ilişkisel veri için(JOIN yaparken işe yarar)
    [JsonIgnore]
    public UcusPlani? UcusPlani { get; set; }


    //Zaman bilgisi : Slider/re-play için önemli
    private DateTime _timestampUtc;
    [Required]
    public DateTime TimestampUtc
    {
        get => DateTime.SpecifyKind(_timestampUtc, DateTimeKind.Utc);
        set => _timestampUtc = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }


    //Koordinatlar (Enlem/Boylam) (Leaflet/OpenStreetMap ile direk uyumludur)
    [Range(-90, 90)] 
    public double Latitude { get; set; } //Enlem (-90 ile 90 arasında)

    [Range(-180, 180)]
    public double Longitude { get; set; } //Boylam (-180 ile 180 arasında)

    //Yön bilgisi (0-360 derece)
    [Range(0, 15000)]
    public double? Altitude { get; set; } //İrtifa (metre cinsinden)

    [Range(0, 360)]
    public double? Heading { get; set; } //Yön (0-360 derece) (pusula yönü)

}
