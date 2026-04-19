namespace LibraryManagementSystem.Entities
{
    public class MembershipType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int MaxBooksAllowed { get; set; }
        public int BorrowDurationDays { get; set; }

        public ICollection<Member> Members { get; set; } = new List<Member>();
    }
}
