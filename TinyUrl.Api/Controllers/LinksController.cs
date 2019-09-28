using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyUrl.Api.AliasKeyService;
using TinyUrl.Data.Models;
using TinyUrl.Data.Repositories;

namespace TinyUrl.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinksController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public LinksController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public class AddLinkResponse
        {
            [Required]
            public string OriginalUrl { get; set; }

            [Required]
            public string Link { get; set; }
        }

        public class AddLinkRequest
        {
            [Required]
            [MaxLength(2048)]
            public string Url { get; set; }

            [MinLength(8)]
            [MaxLength(32)]
            public string Alias { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> AddLink([FromBody] AddLinkRequest request,
                                                 [FromServices] AliasKeyService.AliasKeyService keyService)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest();

            // TODO regex test incoming url for validity

            if (!request.Url.StartsWith("http"))
                request.Url = "http://" + request.Url;
            request.Url = new Uri(request.Url).ToString(); // this will standardize the url and lowercase approprate parts

            AddLinkResponse response = new AddLinkResponse()
            {
                OriginalUrl = request.Url
            };

            string alias = null;
            bool insert = false;
            if (!string.IsNullOrWhiteSpace(request.Alias))
            {
                // check if alias is already used in db
                string originalUrl = await _unitOfWork.LinksRepository.GetOriginalUrlByAlias(request.Alias);
                if (originalUrl != null)
                    return Conflict(); // alias is already used
                
                alias = request.Alias;
                insert = true;
            }
            else
            {
                // first check if the link already exists
                alias = await _unitOfWork.LinksRepository.GetAliasByLink(request.Url);
                if (alias == null)
                {
                    alias = await keyService.GetKey();
                    insert = true;
                }
            }

            if (insert)
            {
                Link link = new Link()
                {
                    Alias = alias,
                    CreationDateTime = DateTime.UtcNow,
                    OriginalUrl = request.Url
                };

                _unitOfWork.LinksRepository.Insert(link);
                await _unitOfWork.SaveAsync();
            }

            // TODO inject base url from configuration for the shortened url 
            response.Link = $"{Request.Scheme}://{Request.Host}/{alias}";

            return Ok(response);
        }
    }
}