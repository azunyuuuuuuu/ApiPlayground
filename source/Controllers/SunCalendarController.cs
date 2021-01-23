using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Innovative.SolarCalculator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiPlayground.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SunCalendarController : ControllerBase
    {
        private readonly ILogger<SunCalendarController> _logger;

        public SunCalendarController(ILogger<SunCalendarController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get([FromQuery] double lat, [FromQuery] double lon)
        {
            var output = Enumerable.Range(0, 365)
                .Select(x => new DateTime(DateTimeOffset.Now.Year, 1, 1) + TimeSpan.FromDays(x))
                .Where(x => x.Year == DateTimeOffset.Now.Year)
                .Select(x => new SolarTimes(new DateTimeOffset(x, DateTimeOffset.Now.Offset), lat, lon))
                .Select(x => $"{x.ForDate}: {x.DawnCivil} - {x.Sunrise} / {x.Sunset} - {x.DuskCivil}");

            return string.Join(Environment.NewLine, output);
        }
    }
}
