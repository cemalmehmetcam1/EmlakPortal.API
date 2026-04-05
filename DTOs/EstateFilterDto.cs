namespace EmlakPortal.API.DTOs
{
    public class EstateFilterDto
    {
        public decimal? MinPrice { get; set; } // Minimum Fiyat
        public decimal? MaxPrice { get; set; } // Maksimum Fiyat
        public string? City { get; set; } // Şehir
        public int? RoomCount { get; set; } // Oda Sayısı
        public int? Status { get; set; } // 1: Satılık, 2: Kiralık
    }
}