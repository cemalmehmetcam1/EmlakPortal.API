namespace EmlakPortal.API.Models
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Örn: Konut, İşyeri, Arsa
        public string Description { get; set; } = string.Empty;

        // Bu kategoriye ait emlak ilanları
        public ICollection<Estate>? Estates { get; set; }
    }
}