using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using ApiPlayground.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiPlayground.Controllers
{
    [ApiController]
    [Route("gist")]
    public class GistController : ControllerBase
    {
        private readonly ILogger<GistController> _logger;
        private readonly IHttpClientFactory _httpclientfactory;

        public GistController(ILogger<GistController> logger, IHttpClientFactory httpclientfactory)
        {
            _logger = logger;
            _httpclientfactory = httpclientfactory;
        }

        [HttpGet()]
        public async Task<string> Get([FromQuery] string user, [FromQuery] string id, [FromQuery] string file)
        {
            if (string.IsNullOrWhiteSpace(user)) throw new ArgumentException($"Argument {nameof(user)} needs to have a value.");
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException($"Argument {nameof(id)} needs to have a value.");
            if (string.IsNullOrWhiteSpace(file)) throw new ArgumentException($"Argument {nameof(file)} needs to have a value.");

            using var client = _httpclientfactory.CreateClient();
            using var context = BrowsingContext.New(Configuration.Default);

            var querypath = $"https://gist.github.com/{user}/{id}";

            var results = await client.GetStringAsync(querypath);

            using var document = await context.OpenAsync(req => req.Content(results));

            var filenames = document.QuerySelectorAll(".file-box .file-header .file-actions a")
                .Select(x => x.Attributes["href"].Value)
                .Select(x => new
                {
                    Url = $"https://gist.github.com{x}",
                    Filename = x.Split("/").Last()
                });

            if (filenames.SingleOrDefault(x => x.Filename == file) == null)
                throw new ArgumentException($"File '{file}' is not in the gist");

            return await client.GetStringAsync(filenames.SingleOrDefault(x => x.Filename == file).Url);
        }
    }
}
