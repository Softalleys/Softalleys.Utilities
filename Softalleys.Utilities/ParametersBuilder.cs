using System.Collections.Specialized;
using System.Web;

namespace Softalleys.Utilities;

/// <summary>
/// Provides a builder for constructing and manipulating query strings or URI fragment parts.
/// </summary>
public class ParametersBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParametersBuilder"/> class.
    /// </summary>
    /// <param name="valuesString">A string representing the initial query string or URI fragment.</param>
    public ParametersBuilder(string valuesString = "")
    {
        _values = HttpUtility.ParseQueryString(valuesString);
    }

    private readonly NameValueCollection _values;

    /// <summary>
    /// Gets or sets the value associated with the specified parameter name.
    /// </summary>
    /// <param name="name">The name of the parameter to get or set.</param>
    /// <returns>The value associated with the specified name.</returns>
    public string? this[string name]
    {
        get => _values[name];
        set => _values[name] = value;
    }

    /// <summary>
    /// Returns a string that represents the current query string or URI fragment.
    /// </summary>
    /// <returns>A string that represents the current state of the builder.</returns>
    public override string ToString() => _values.ToString() ?? string.Empty;

    /// <summary>
    /// Clears all the parameters from the builder.
    /// </summary>
    public void Clear() => _values.Clear();
}
