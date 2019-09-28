using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TinyUrl.Data.Models
{
    public class TinyUrlDbContext : DbContext
    {
        public virtual DbSet<Link> Links { get; set; }
        public virtual DbSet<AliasKey> AliasKeys { get; set; }

        public TinyUrlDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Link>(entity =>
            {
                entity.HasIndex(e => e.Alias)
                      .IsUnique();

                entity.HasIndex(e => e.OriginalUrl); // depending this can become an extremly large index, but most urls are short. should look into alternative ways to query on this field if performance or data size becomes an issue.
            });
        }
    }
}
