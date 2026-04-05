using EmlakPortal.API.Models.Enums;

namespace EmlakPortal.API.Models
{
    public class Estate : BaseEntity
    {
        public string Title { get; set; } = string.Empty; // İlan Başlığı
        public string Description { get; set; } = string.Empty; // İlan Detayı
        public decimal Price { get; set; } // Fiyat
        public int RoomCount { get; set; } // Oda Sayısı (Örn: 3+1 yerine şimdilik 3 veya 4 gibi tutulabilir)
        public int SquareMeters { get; set; } // Metrekare
        public string City { get; set; } = string.Empty; // Şehir
        public string Address { get; set; } = string.Empty; // Açık Adres

        public EstateStatus Status { get; set; } // Satılık mı, Kiralık mı?

        // İlişkiler
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string AppUserId { get; set; } = string.Empty; // İlanı ekleyen kullanıcı (Admin/Emlakçı)
        public AppUser? AppUser { get; set; }

        public string? ImageUrl { get; set; } // Vitrin fotoğrafının yolu
    }
}