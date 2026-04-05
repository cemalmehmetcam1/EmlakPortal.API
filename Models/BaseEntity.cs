namespace EmlakPortal.API.Models
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true; // Silme işlemi yerine pasife çekmek için (Soft Delete)
    }
}