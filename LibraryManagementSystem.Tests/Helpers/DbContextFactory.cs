using LibraryManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Tests.Helpers
{
    public static class DbContextFactory
    {
        // Each call returns a fresh isolated database — no state leaks between tests
        public static ApplicationDbContext Create() =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
    }
}
