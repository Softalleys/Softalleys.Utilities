namespace Softalleys.Utilities;


/// <summary>
/// A wrapper around System.UriBuilder, providing enhanced functionality for URI manipulation,
/// specifically for handling query strings and fragments.
/// </summary>
public class UriBuilderWithQuery
{
    /// <summary>
    /// Initializes a new instance of the UriBuilder class with the specified Uri instance.
    /// </summary>
    /// <param name="uri">The Uri instance to use as the base of the UriBuilder.</param>
    public UriBuilderWithQuery(Uri uri)
        : this(new UriBuilder(uri))
    {
    }

    /// <summary>
    /// Initializes a new instance of the UriBuilder class with the specified URI string.
    /// </summary>
    /// <param name="uri">A URI string to use as the base of the UriBuilder.</param>
    public UriBuilderWithQuery(string uri)
        : this(new UriBuilder(uri))
    {
    }

    /// <summary>
    /// Internal constructor that initializes the UriBuilder with a System.UriBuilder instance.
    /// </summary>
    /// <param name="builder">The System.UriBuilder instance to wrap.</param>
    private UriBuilderWithQuery(UriBuilder builder)
    {
        _builder = builder;
        Query = new ParametersBuilder(_builder.Query);
        Fragment = new ParametersBuilder(_builder.Fragment);
    }

    private readonly UriBuilder _builder;

    /// <summary>
    /// Gets the ParametersBuilder for the query string.
    /// </summary>
    public ParametersBuilder Query { get; }

    /// <summary>
    /// Gets the ParametersBuilder for the fragment part of the URI.
    /// </summary>
    public ParametersBuilder Fragment { get; }

    /// <summary>
    /// Gets the URI constructed by the UriBuilder.
    /// </summary>
    public Uri Uri
    {
        get
        {
            _builder.Query = Query.ToString();
            _builder.Fragment = Fragment.ToString();
            return _builder.Uri;
        }
    }

    /// <summary>
    /// Converts a UriBuilder instance to a Uri.
    /// </summary>
    /// <param name="builder">The UriBuilder instance to convert.</param>
    public static implicit operator Uri(UriBuilderWithQuery builder) => builder.Uri;

    /// <summary>
    /// Converts a UriBuilder instance to a string.
    /// </summary>
    /// <param name="builder">The UriBuilder instance to convert.</param>
    public static implicit operator string(UriBuilderWithQuery builder) => builder.Uri.ToString();
}
