using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TinyUrl.Data.Models;

namespace TinyUrl.Data.Repositories
{
    public class LinksRepository : EFBaseRepository<Link>, ILinksRepository
    {
        public LinksRepository(TinyUrlDbContext context) : base(context)
        {
        }

        public Task<string> GetAliasByLink(string link)
        {
            return Query().Where(o => o.OriginalUrl == link).Take(1).Select(o => o.Alias).FirstOrDefaultAsync();
        }

        public Task<string> GetOriginalUrlByAlias(string alias)
        {
            return Query().Where(o => o.Alias == alias).Take(1).Select(o => o.OriginalUrl).FirstOrDefaultAsync();
        }
    }

    public interface ILinksRepository : IBaseRepository<Link>
    {
        Task<string> GetAliasByLink(string link);
        Task<string> GetOriginalUrlByAlias(string alias);
    }
}
