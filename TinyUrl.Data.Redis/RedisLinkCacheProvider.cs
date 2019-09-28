using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TinyUrl.Data.Redis
{
    public class RedisLinkCacheProvider : ILinkCacheProvider
    {
        public string RedisNamespace { get; }

        protected ConnectionMultiplexer Connection { get; set; }
        protected IServer Server { get; set; }
        protected IDatabase Database { get; set; }

        public RedisLinkCacheProvider(string connectionString, string redisNamespace = "link")
        {
            RedisNamespace = redisNamespace;
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            Connection = ConnectionMultiplexer.Connect(options);
            Server = Connection.GetServer(options.EndPoints.First());
            Database = Connection.GetDatabase(options.DefaultDatabase ?? 0);
        }

        public async Task<string> GetLinkForAlias(string alias)
        {
            RedisValue value = await Database.StringGetAsync(GetKeyForAlias(alias));
            return value.HasValue ? value.ToString() : null;
        }

        public Task CacheLinkForAlias(string alias, string link)
        {
            return Database.StringSetAsync(GetKeyForAlias(alias), link);
        }

        private string GetKeyForAlias(string alias)
        {
            return $"{RedisNamespace}:{alias}";
        }
    }
}
