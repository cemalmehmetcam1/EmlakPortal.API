namespace EmlakPortal.API.DTOs
{
    public class EstateUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int RoomCount { get; set; }
        public int SquareMeters { get; set; }
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Status { get; set; } // 1: Satılık, 2: Kiralık
        public int CategoryId { get; set; }
        public bool IsActive { get; set; }
    }
}