using LlmCommon;
using LlmCommon.Abstractions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LlmBackend.Auth
{
    class AppDbContext(DbContextOptions<AppDbContext> options, IEventBus eventBus) : IdentityDbContext<AppUser>(options), IUnitOfWork
    {
        public DbSet<DbView> Views { get; set; }
        public DbSet<DbEntity> Entities { get; set; }
        private readonly List<Event> eventsToBroadcast = new List<Event>();

        public void AddEventsFrom(Entity e) {
            eventsToBroadcast.AddRange(e.DequeueUncommittedEvents());
        }

        public async Task<int> StoreAsync(CancellationToken cancellationToken = default)
        {
            var i = await this.SaveChangesAsync();
            foreach (var e in eventsToBroadcast) {
                await eventBus.Publish(e);
            }
            eventsToBroadcast.Clear();
            return i;
        }
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
