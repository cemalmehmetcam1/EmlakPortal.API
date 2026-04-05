using EmlakPortal.API.Data;
using EmlakPortal.API.Models;
using EmlakPortal.API.Repositories;
using EmlakPortal.API.Services; // Token servisi için
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT için
using Microsoft.IdentityModel.Tokens; // JWT için
using System.Text; // Encoding için
using Microsoft.OpenApi.Models; // Swagger için

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// --- CORS AYARLARI (MVC AJAX Ýsteklerine Ýzin Verme) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddEndpointsApiExplorer();

// --- 1. SWAGGER VE JWT KÝLÝT AYARLARI ---
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Emlak Portal API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token'ýnýzý direkt aþaðýya yapýþtýrýn (Baþýna 'Bearer ' yazmanýza gerek yok)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// --- 2. VERÝ TABANI BAÐLANTISI ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 3. IDENTITY (ÜYELÝK) SÝSTEMÝ ---
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- 4. REPOSITORY VE SERVÝS KAYITLARI ---
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITokenService, TokenService>(); // Token üreten servisi tanýttýk

// --- 5. JWT KÝMLÝK DOÐRULAMA AYARLARI ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // EKLENECEK SATIR: wwwroot klasörünü dýþarý açar
app.UseHttpsRedirection(); // Bu zaten var

// DÝKKAT: Sýralama çok önemli! Önce Kimlik Doðrulama, sonra Yetki Kontrolü
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// --- DATA SEEDING (SÝSTEM ÝLK AYAÐA KALKARKEN ÇALIÞACAK KODLAR) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();

        // 1. Sistemdeki temel rolleri oluþtur
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new AppRole { Name = roleName });
            }
        }

        // 2. Eðer veritabanýnda hiç Admin yoksa, ilk kurucu hesabý otomatik oluþtur
        string adminEmail = "admin@emlakportal.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = "admin", // Kullanýcý adýmýz: admin
                Email = adminEmail,
                FullName = "Sistem Yöneticisi",
                EmailConfirmed = true
            };

            // Þifreyi Admin123! olarak belirliyoruz
            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Veri tohumlama sýrasýnda hata oluþtu: " + ex.Message);
    }
}

app.Run();