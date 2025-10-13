using Microsoft.EntityFrameworkCore;
using UcakTakip.Api.Data;
using UcakTakip.Api.Middlewares;
var builder = WebApplication.CreateBuilder(args);


// React API'ye ERÝÞEBÝLSÝN diye CORS ayarlarýný yapýyoruz.
//CORS (Cross-Origin Resource Sharing) denen sistem, backend’in hangi adreslerden gelen istekleri kabul edeceðini belirliyor.
builder.Services.AddCors(o => o.AddPolicy("ui", p =>
    p.WithOrigins("http://localhost:3000",
        "http://localhost:5173",
        "http://localhost:5174",      // BUNU EKLE
        "http://127.0.0.1:5173",
        "http://127.0.0.1:5174")
     .AllowAnyHeader()
     .AllowAnyMethod()));


// SQL Baðlantýsý ve DbContext yapýlandýrmasý. Böylece ortam deðiþtirmek sadece bu satýrý deðiþtirerek kolay olur. PostgreSQL, MySQL vs.
//Bu options ile DbContext'e baðlantý bilgisini veriyoruz. AppDbContext sýnýfýnda. 

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

var app = builder.Build();
app.UseGenelHataYakalayici();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ui");
//CORS politikasý uygulansýn

app.UseAuthorization();

app.MapControllers();

app.Run();
