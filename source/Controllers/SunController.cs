using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ApiPlayground.Models;
using Innovative.SolarCalculator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiPlayground.Controllers
{
    [ApiController]
    [Route("sun")]
    public class SunController : ControllerBase
    {
        private readonly ILogger<SunController> _logger;

        public SunController(ILogger<SunController> logger)
        {
            _logger = logger;
        }

        [HttpGet("calendar.ics")]
        [Produces("text/calendar")]
        public IEnumerable<CalendarEvent> Get([FromQuery] double lat, [FromQuery] double lon)
        {
            var results = Enumerable.Range(0, 365)
                .Select(x => new DateTime(DateTimeOffset.Now.Year, 1, 1) + TimeSpan.FromDays(x))
                .Where(x => x.Year == DateTimeOffset.Now.Year)
                .Select(x => new SolarTimes(new DateTimeOffset(x, DateTimeOffset.Now.Offset), lat, lon)).Select(x => new
                {
                    Date = x.ForDate,
                    SunriseBegin = x.DawnCivil - TimeSpan.FromHours((double)x.TimeZoneOffset),
                    SunriseEnd = x.Sunrise - TimeSpan.FromHours((double)x.TimeZoneOffset),
                    SunsetBegin = x.Sunset - TimeSpan.FromHours((double)x.TimeZoneOffset),
                    SunsetEnd = x.DuskCivil - TimeSpan.FromHours((double)x.TimeZoneOffset),
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                }).Select(x => new
                {
                    SunriseEvent = new CalendarEvent(
                         Summary: "🌅",
                         Start: x.SunriseBegin,
                         End: x.SunriseEnd),
                    SunsetEvent = new CalendarEvent(
                         Summary: "🌇",
                         Start: x.SunsetBegin,
                         End: x.SunsetEnd)
                });

            foreach (var item in results)
            {
                yield return item.SunriseEvent;
                yield return item.SunsetEvent;
            }
        }
    }
}
