namespace LibraryManagementSystem.DTOs.Member
{
    public class MemberDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime MembershipDate { get; set; }
        public bool IsActive { get; set; }
        public int MembershipTypeId { get; set; }
        public string MembershipTypeName { get; set; } = string.Empty;
    }
}
