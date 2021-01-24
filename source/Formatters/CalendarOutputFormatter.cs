using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using ApiPlayground.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace ApiPlayground.Formatter
{
    public class CalendarOutputFormatter : TextOutputFormatter
    {
        public CalendarOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/calendar"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(CalendarEvent).IsAssignableFrom(type) ||
                typeof(IEnumerable<CalendarEvent>).IsAssignableFrom(type);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;
            var serviceProvider = httpContext.RequestServices;

            var logger = serviceProvider.GetRequiredService<ILogger<CalendarOutputFormatter>>();
            var buffer = new StringBuilder();

            AddCalendarEventsHeader(buffer, logger);

            if (context.Object is IEnumerable<CalendarEvent> contacts)
            {
                foreach (var contact in contacts)
                {
                    FormatCalendarEvent(buffer, contact, logger);
                }
            }
            else
            {
                FormatCalendarEvent(buffer, (CalendarEvent)context.Object, logger);
            }

            AddCalendarEventsFooter(buffer, logger);

            await httpContext.Response.WriteAsync(buffer.ToString());
        }

        private static void AddCalendarEventsHeader(StringBuilder buffer, ILogger<CalendarOutputFormatter> logger)
        {
            buffer.AppendLine("BEGIN:VCALENDAR");
            buffer.AppendLine("VERSION:2.0");
            buffer.AppendLine("PRODID:-//azunyuuuuuuu/cal//NONSGML v1.0//EN");
            buffer.AppendLine("X-WR-CALNAME:Calendar Events");

            logger.LogInformation("Started writing Calendar Events");
        }

        private static void AddCalendarEventsFooter(StringBuilder buffer, ILogger<CalendarOutputFormatter> logger)
        {
            buffer.AppendLine("END:VCALENDAR");

            logger.LogInformation("Finished writing Calendar Events");
        }

        private static void FormatCalendarEvent(StringBuilder buffer, CalendarEvent @event, ILogger<CalendarOutputFormatter> logger)
        {
            buffer.AppendLine("BEGIN:VEVENT");

            // buffer.AppendLine($"UID:-");
            // buffer.AppendLine($"CREATED:-");
            // buffer.AppendLine($"LAST-MODIFIED:-");

            buffer.AppendLine($"DTSTART:{(@event.Start).ToString("yyyyMMdd'T'HHmmss'Z'", DateTimeFormatInfo.InvariantInfo)}");
            buffer.AppendLine($"DTEND:{(@event.End).ToString("yyyyMMdd'T'HHmmss'Z'", DateTimeFormatInfo.InvariantInfo)}");
            buffer.AppendLine($"SUMMARY:{@event.Summary}");
            buffer.AppendLine($"DESCRIPTION:");
            buffer.AppendLine($"STATUS:CONFIRMED");
            buffer.AppendLine($"TRANSP:TRANSPARENT");
            buffer.AppendLine($"LOCATION:");
            buffer.AppendLine($"SEQUENCE:1");

            buffer.AppendLine($"X-MICROSOFT-CDO-BUSYSTATUS:FREE");

            buffer.AppendLine("END:VEVENT");

            logger.LogInformation($"Writing '{@event.Summary}' ({@event.Start} - {@event.End})");
        }
    }
}
