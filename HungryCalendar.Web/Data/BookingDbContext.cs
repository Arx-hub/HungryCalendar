using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace HungryCalendar.Web.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options)
            : base(options)
        {
        }

        public DbSet<DbReservation> Reservations { get; set; }
        public DbSet<DbDisabledSlot> DisabledSlots { get; set; }
    }

    public class DbReservation
    {
        public int Id { get; set; }
        [Required]
        public string Date { get; set; } = string.Empty;
        [Required]
        public string Time { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Phone { get; set; } = string.Empty;
        public int GroupSize { get; set; }
    }

    public class DbDisabledSlot
    {
        public int Id { get; set; }
        [Required]
        public string Date { get; set; } = string.Empty;
        [Required]
        public string Time { get; set; } = string.Empty;
    }
}
