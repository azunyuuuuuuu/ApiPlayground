using System;

namespace ApiPlayground.Models
{
    public record CalendarEvent(string Summary, DateTime Start, DateTime End);
}
