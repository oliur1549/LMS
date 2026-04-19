namespace LibraryManagementSystem.Entities
{
    // Abstraction: Common base for all entities (OOP - Inheritance)
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
