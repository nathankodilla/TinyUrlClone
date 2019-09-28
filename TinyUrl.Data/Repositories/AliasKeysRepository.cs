using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TinyUrl.Data.Models;

namespace TinyUrl.Data.Repositories
{
    public class AliasKeysRepository : EFBaseRepository<AliasKey>, IAliasKeysRepository
    {
        public AliasKeysRepository(TinyUrlDbContext context) : base(context)
        {
        }

        public Task<List<AliasKey>> GetRandomKeys(int count = 100)
        {
            return Query().OrderBy(o => Guid.NewGuid()).Take(count).ToListAsync();
        }
    }

    public interface IAliasKeysRepository : IBaseRepository<AliasKey>
    {
        Task<List<AliasKey>> GetRandomKeys(int count = 100);
    }
}
