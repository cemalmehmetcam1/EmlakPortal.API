using EmlakPortal.API.DTOs;
using EmlakPortal.API.Models;
using EmlakPortal.API.Models.Enums;
using EmlakPortal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstateController : ControllerBase
    {
        private readonly IGenericRepository<Estate> _estateRepo;

        public EstateController(IGenericRepository<Estate> estateRepo)
        {
            _estateRepo = estateRepo;
        }

        // GENEL ARA YÜZ: Tüm aktif ilanları listeler (Giriş yapmaya gerek yok)
        [HttpGet]
        public async Task<IActionResult> GetEstates()
        {
            var estates = await _estateRepo.AsQueryable()
                .Include(e => e.Category) // Kategorisini dahil et
                .Include(e => e.AppUser) // Ekleyen emlakçıyı dahil et
                .Where(e => e.IsActive)
                .Select(e => new EstateDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Price = e.Price,
                    RoomCount = e.RoomCount,
                    SquareMeters = e.SquareMeters,
                    City = e.City,
                    StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                    CategoryName = e.Category!.Name,
                    AddedBy = e.AppUser!.FullName,
                    CreatedDate = e.CreatedDate
                }).ToListAsync();

            return Ok(estates);
        }

        // GENEL ARA YÜZ: İlanın detayına girme (ID'ye göre)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEstateById(int id)
        {
            var estate = await _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

            if (estate == null)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            var estateDto = new EstateDto
            {
                Id = estate.Id,
                Title = estate.Title,
                Price = estate.Price,
                RoomCount = estate.RoomCount,
                SquareMeters = estate.SquareMeters,
                City = estate.City,
                StatusName = estate.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                CategoryName = estate.Category!.Name,
                AddedBy = estate.AppUser!.FullName,
                CreatedDate = estate.CreatedDate
            };

            return Ok(estateDto);
        }

        // YÖNETİCİ PANELİ: Yeni İlan Ekleme (Sadece Admin yetkililer yapabilir)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddEstate(EstateCreateDto model)
        {
            // Token'dan giriş yapan kişinin ID'sini alıyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var estate = new Estate
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                RoomCount = model.RoomCount,
                SquareMeters = model.SquareMeters,
                City = model.City,
                Address = model.Address,
                Status = (EstateStatus)model.Status,
                CategoryId = model.CategoryId,
                AppUserId = userId!,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            await _estateRepo.AddAsync(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla eklendi." });
        }

        // YÖNETİCİ PANELİ: İlan Silme (Soft Delete)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstate(int id)
        {
            var estate = await _estateRepo.GetByIdAsync(id);
            if (estate == null) return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            // Gerçekten silmiyoruz, pasife çekiyoruz
            estate.IsActive = false;
            _estateRepo.Update(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla yayından kaldırıldı." });
        }
        // YÖNETİCİ PANELİ: İlan Güncelleme
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> UpdateEstate(EstateUpdateDto model)
        {
            var estate = await _estateRepo.GetByIdAsync(model.Id);
            if (estate == null) return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            estate.Title = model.Title;
            estate.Description = model.Description;
            estate.Price = model.Price;
            estate.RoomCount = model.RoomCount;
            estate.SquareMeters = model.SquareMeters;
            estate.City = model.City;
            estate.Address = model.Address;
            estate.Status = (EstateStatus)model.Status;
            estate.CategoryId = model.CategoryId;
            estate.IsActive = model.IsActive;

            _estateRepo.Update(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "İlan başarıyla güncellendi." });
        }

        // GENEL ARA YÜZ: Kategoriye Göre İlanları Filtreleme (Örn: Sadece İşyerlerini getir)
        [HttpGet("ByCategory/{categoryId}")]
        public async Task<IActionResult> GetEstatesByCategory(int categoryId)
        {
            var estates = await _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Where(e => e.IsActive && e.CategoryId == categoryId)
                .Select(e => new EstateDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Price = e.Price,
                    RoomCount = e.RoomCount,
                    SquareMeters = e.SquareMeters,
                    City = e.City,
                    StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                    CategoryName = e.Category!.Name,
                    AddedBy = e.AppUser!.FullName,
                    CreatedDate = e.CreatedDate
                }).ToListAsync();

            return Ok(estates);
        }
        // GENEL ARA YÜZ: Gelişmiş İlan Filtreleme (Sahibinden tarzı detaylı arama)
        [HttpPost("Filter")]
        public async Task<IActionResult> GetEstatesByFilter(EstateFilterDto filter)
        {
            // Önce tüm aktif ilanları sorgu olarak başlatıyoruz (Veritabanından henüz çekmiyoruz!)
            var query = _estateRepo.AsQueryable()
                .Include(e => e.Category)
                .Include(e => e.AppUser)
                .Where(e => e.IsActive);

            // Gelen filtrelere göre sorguya "EĞER" şartları ekliyoruz:
            if (filter.MinPrice.HasValue)
                query = query.Where(e => e.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(e => e.Price <= filter.MaxPrice.Value);

            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(e => e.City.ToLower().Contains(filter.City.ToLower()));

            if (filter.RoomCount.HasValue)
                query = query.Where(e => e.RoomCount == filter.RoomCount.Value);

            if (filter.Status.HasValue)
                query = query.Where(e => (int)e.Status == filter.Status.Value);

            // Filtreler eklendikten sonra nihayet veriyi çekip DTO'ya dönüştürüyoruz
            var estates = await query.Select(e => new EstateDto
            {
                Id = e.Id,
                Title = e.Title,
                Price = e.Price,
                RoomCount = e.RoomCount,
                SquareMeters = e.SquareMeters,
                City = e.City,
                StatusName = e.Status == EstateStatus.Satilik ? "Satılık" : "Kiralık",
                CategoryName = e.Category!.Name,
                AddedBy = e.AppUser!.FullName,
                CreatedDate = e.CreatedDate
            }).ToListAsync();

            if (estates.Count == 0)
                return Ok(new ResultDto { Status = true, Message = "Kriterlerinize uygun ilan bulunamadı.", Data = estates });

            return Ok(estates);
        }
        // YÖNETİCİ PANELİ: İlana Vitrin Fotoğrafı Yükleme
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/ImageUpload")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ResultDto { Status = false, Message = "Lütfen bir fotoğraf seçin." });

            // 1. İlanı bul
            var estate = await _estateRepo.GetByIdAsync(id);
            if (estate == null)
                return NotFound(new ResultDto { Status = false, Message = "İlan bulunamadı." });

            // 2. Klasörü ayarla (wwwroot/images)
            var webRootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 3. Dosyayı benzersiz bir isimle kaydet (Çakışmaları önlemek için Guid kullanıyoruz)
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. İlanın veritabanındaki resim yolunu güncelle
            estate.ImageUrl = "/images/" + uniqueFileName;
            _estateRepo.Update(estate);
            await _estateRepo.SaveAsync();

            return Ok(new ResultDto { Status = true, Message = "Vitrin fotoğrafı başarıyla yüklendi.", Data = estate.ImageUrl });
        }
    }
}