using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
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
                .Select(x => new SolarTimes(new DateTimeOffset(x, DateTimeOffset.Now.Offset), lat, lon));
            // .Select(x => $"{x.ForDate}: {x.DawnCivil} - {x.Sunrise} / {x.Sunset} - {x.DuskCivil}");

            var corrected = output.Select(x => new
            {
                Date = x.ForDate,
                SunriseBegin = x.DawnCivil - TimeSpan.FromHours((double)x.TimeZoneOffset),
                SunriseEnd = x.Sunrise - TimeSpan.FromHours((double)x.TimeZoneOffset),
                SunsetBegin = x.Sunset - TimeSpan.FromHours((double)x.TimeZoneOffset),
                SunsetEnd = x.DuskCivil - TimeSpan.FromHours((double)x.TimeZoneOffset),
                Latitude = x.Latitude,
                Longitude = x.Longitude
            })
            .ToList();

            var vcalendarheader =
                "BEGIN:VCALENDAR\n" +
                "VERSION:2.0\n" +
                "PRODID:-//azunyuuuuuuu/cal//NONSGML v1.0//EN\n" +
                "X-WR-CALNAME:Sunset Sunrise Events\n" +
                "METHOD:PUBLISH\n";

            var vcalendarfooter = "END:VCALENDAR\n";

            var temp = corrected.Select(x =>
                "BEGIN:VEVENT\n" +
                // "UID:uid1@example.com\n" +
                // "DTSTAMP:19970714T170000Z\n" +
                // "ORGANIZER;CN=John Doe:MAILTO:john.doe@example.com\n" +
                // $"DTSTART:19970714T170000Z\n" +
                $"DTSTART:{(x.SunriseBegin).ToString("yyyyMMdd'T'HHmmss'Z'", DateTimeFormatInfo.InvariantInfo)}\n" +
                // $"DTEND:19970715T035959Z\n" +
                $"DTEND:{x.SunriseEnd.ToString("yyyyMMdd'T'HHmmss'Z'", DateTimeFormatInfo.InvariantInfo)}\n" +
                "SUMMARY:Sunrise\n" +
                $"GEO:{x.Latitude.ToString("F5", CultureInfo.InvariantCulture)};{x.Longitude.ToString("F5", CultureInfo.InvariantCulture)}\n" +
                "END:VEVENT\n" +
                "BEGIN:VEVENT\n" +
                // "UID:uid1@example.com\n" +
                // "DTSTAMP:19970714T170000Z\n" +
                // "ORGANIZER;CN=John Doe:MAILTO:john.doe@example.com\n" +
                // $"DTSTART:19970714T170000Z\n" +
                $"DTSTART:{x.SunsetBegin.ToString("yyyyMMdd'T'HHmmss'Z'", DateTimeFormatInfo.InvariantInfo)}\n" +
                // $"DTEND:19970715T035959Z\n" +
                $"DTEND:{x.SunsetEnd.ToString("yyyyMMdd'T'HHmmss'Z'", DateTimeFormatInfo.InvariantInfo)}\n" +
                "SUMMARY:Sunset\n" +
                $"GEO:{x.Latitude.ToString("F5", CultureInfo.InvariantCulture)};{x.Longitude.ToString("F5", CultureInfo.InvariantCulture)}\n" +
                "END:VEVENT\n"
                );


            return string.Join("", vcalendarheader, string.Join("", temp), vcalendarfooter);
        }
    }
}
