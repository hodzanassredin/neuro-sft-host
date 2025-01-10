using LlmCommon;
using LlmCommon.Abstractions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LlmBackend.Auth
{
    class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options), IUnitOfWork
    {
        public DbSet<DbView> Views { get; set; }
        public DbSet<DbEntity> Entities { get; set; }
    }

    public class DbView : IDisposable {
        public string Id { get; set; }
        public JsonDocument View { get; set; }
        public void Dispose() => View?.Dispose();
    }

    public class DbEntity : IDisposable
    {
        public string Id { get; set; }
        public JsonDocument Entity { get; set; }
        public void Dispose() => Entity?.Dispose();
    }
}
