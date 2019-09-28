using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TinyUrl.Data.Models;

namespace TinyUrl.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public TinyUrlDbContext Context { get; private set; }

        private ILinksRepository linksRepo;
        public ILinksRepository LinksRepository => linksRepo ?? (linksRepo = new LinksRepository(Context));

        private IAliasKeysRepository aliasKeysRepo;
        public IAliasKeysRepository AliasKeysRepository => aliasKeysRepo ?? (aliasKeysRepo = new AliasKeysRepository(Context));

        public UnitOfWork(TinyUrlDbContext context)
        {
            Context = context;
        }

        public Task SaveAsync()
        {
            return Context.SaveChangesAsync();
        }
    }

    public interface IUnitOfWork
    {
        TinyUrlDbContext Context { get; }
        ILinksRepository LinksRepository { get; }
        IAliasKeysRepository AliasKeysRepository { get; }

        Task SaveAsync();
    }
}
