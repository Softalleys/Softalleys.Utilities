using System.ComponentModel.DataAnnotations;

namespace Softalleys.Utilities.Attributes;

/// <summary>
/// Validation attribute to check if a date is before a specified date.
/// </summary>
public class BeforeAtAttribute : ValidationAttribute
{
    private readonly DateTimeOffset _date;

    /// <summary>
    /// Initializes a new instance of the <see cref="BeforeAtAttribute"/> class.
    /// </summary>
    /// <param name="date">The date to compare against. Can be "now", "utcnow", or a specific date string.</param>
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

    /// <summary>
    /// Determines whether the specified value is valid.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>true if the value is valid; otherwise, false.</returns>
    public override bool IsValid(object? value)
    {
        if (value == null) return true;

        if (value is DateTimeOffset date) return date < _date;

        if (value is DateTime dateTime) return dateTime < _date;

        if (value is DateOnly dateOnly) return dateOnly.ToDateTime(TimeOnly.MinValue) < _date;

        return false;
    }
}