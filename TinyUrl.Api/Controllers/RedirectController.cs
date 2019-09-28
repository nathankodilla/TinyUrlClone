using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TinyUrl.Data;
using TinyUrl.Data.Repositories;

namespace TinyUrl.Api.Controllers
{
    [Route("")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILinkCacheProvider _linkCache;
        private readonly ILogger _logger;

        public RedirectController(IUnitOfWork unitOfWork,
                                  ILinkCacheProvider linkCache,
                                  ILogger<RedirectController> logger)
        {
            _unitOfWork = unitOfWork;
            _linkCache = linkCache;
            _logger = logger;
        }

        [HttpGet("{alias}")]
        public async Task<IActionResult> RedirectForAlias([FromRoute] string alias)
        {
            // check if alias is in cache
            string url = await _linkCache.GetLinkForAlias(alias);
            if (url == null)
            {
                url = await _unitOfWork.LinksRepository.GetOriginalUrlByAlias(alias);
                if (url == null)
                {
                    _logger.LogInformation($"Alias {alias} redirection requested but was not found in the database");
                    return Redirect($"{Request.Scheme}://{Request.Host}"); // redirect to base page
                }

                await _linkCache.CacheLinkForAlias(alias, url); // for quicker response times we could look at not waiting for the await, however redis is pretty fast to begin with
            }

            return Redirect(url);
        }
    }
}