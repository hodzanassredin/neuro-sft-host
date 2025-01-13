using LlmCommon.Abstractions;
using LlmCommon;
using LlmBackend.Auth;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LlmBackend.Infrastructure
{
    class DbEntityStorage: IEntityStorage
    {
        private readonly AppDbContext ctx;

        public DbEntityStorage(AppDbContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<T?> Load<T>(Ids.Id id) where T : Entity
        {
            var res = await ctx.Entities.AsNoTracking().SingleOrDefaultAsync(x=>x.Id == id.ToString());
            if (res == null) {
                return null;
            }
            return res.Entity.Deserialize<T>();
        }

        public async Task Remove(Entity entity)
        {
            DbEntity toDelete = new DbEntity() { Id = entity.Id.ToString() };
            ctx.Entities.Entry(toDelete).State = EntityState.Deleted;
        }

        public async Task Upsert(Entity entity)
        {
            var res = await ctx.Entities.AsNoTracking().SingleOrDefaultAsync(x => x.Id == entity.Id.ToString());
            if (res == null)
            {
                res = new DbEntity()
                {
                    Id = entity.Id.ToString(),
                    Entity = JsonSerializer.SerializeToDocument(entity, entity.GetType())
                };
                ctx.Entities.Add(res);
            }
            else {
                res.Entity = JsonSerializer.SerializeToDocument(entity, entity.GetType());
                ctx.Entities.Update(res);
            }
        }
    }
}
