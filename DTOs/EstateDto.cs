namespace EmlakPortal.API.DTOs
{
    public class EstateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string City { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty; // Satılık veya Kiralık yazacak
        public string CategoryName { get; set; } = string.Empty; // Konut, İşyeri vs. yazacak
        public string AddedBy { get; set; } = string.Empty; // İlanı ekleyen emlakçının adı
        public DateTime CreatedDate { get; set; }

        public string? ImageUrl { get; set; }
    }
}