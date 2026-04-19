namespace LibraryManagementSystem.Entities
{
    // Inherits from BaseEntity (OOP - Inheritance)
    public class Library : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Navigation Property (One-to-Many: Library has many Books)
        public ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
