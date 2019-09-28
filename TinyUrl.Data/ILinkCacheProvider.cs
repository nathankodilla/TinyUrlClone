using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TinyUrl.Data
{
    public interface ILinkCacheProvider
    {
        Task<string> GetLinkForAlias(string alias);
        Task CacheLinkForAlias(string alias, string link);
    }
}
