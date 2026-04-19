using LibraryManagementSystem.Enums;

namespace LibraryManagementSystem.Entities
{
    public class Reservation : BaseEntity
    {
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public DateTime ReservedDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public bool IsExpired() => Status == ReservationStatus.Pending && DateTime.UtcNow > ExpiryDate;

        public void Cancel()
        {
            if (Status != ReservationStatus.Pending)
                throw new InvalidOperationException("Only pending reservations can be cancelled.");
            Status = ReservationStatus.Cancelled;
        }

        public void Fulfil()
        {
            if (Status != ReservationStatus.Pending)
                throw new InvalidOperationException("Only pending reservations can be fulfilled.");
            Status = ReservationStatus.Fulfilled;
        }

        public void CheckAndExpire()
        {
            if (IsExpired())
                Status = ReservationStatus.Expired;
        }
    }
}
