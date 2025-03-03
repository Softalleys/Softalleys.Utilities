using System.ComponentModel.DataAnnotations;

namespace Softalleys.Utilities.Attributes;

public class BeforeAtAttribute : ValidationAttribute
{
    private readonly DateTimeOffset _date;

    public BeforeAtAttribute(string date)
    {
        date = date.ToLowerInvariant().Trim();

        if (date.StartsWith("now"))
        {
            _date = DateTimeOffset.Now;
            date = date[3..].Trim();
        }
        else if (date.StartsWith("utcnow"))
        {
            _date = DateTimeOffset.UtcNow;
            date = date[6..].Trim();
        }
        else
        {
            _date = DateTimeOffset.Parse(date);
        }

        if (TimeSpan.TryParse(date, out var timeSpan))
        {
            _date = _date.Add(timeSpan);
        }
        else
        {
            // if contains a year format like 18y, 2y, 3y, -18y, -3y, etc.
            if (date.EndsWith("y"))
            {
                var years = int.Parse(date[..^1]);
                _date = _date.AddYears(years);
            }
        }
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (value is DateTimeOffset date) return date < _date;

        if (value is DateTime dateTime) return dateTime < _date;

        if (value is DateOnly dateOnly) return dateOnly.ToDateTime(TimeOnly.MinValue) < _date;

        return false;
    }
}