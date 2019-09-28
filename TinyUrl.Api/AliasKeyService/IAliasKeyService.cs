using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TinyUrl.Api.AliasKeyService
{
    public interface IAliasKeyService
    {
        Task<string> GetKey();
    }
}
