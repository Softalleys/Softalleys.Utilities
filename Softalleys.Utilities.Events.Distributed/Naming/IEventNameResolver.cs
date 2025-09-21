namespace Softalleys.Utilities.Events.Distributed.Naming;

public interface IEventNameResolver
{
    string GetName(Type eventType);
    int GetVersion(Type eventType);
}

public enum NameCase
{
    KebabCase,
    CamelCase,
    PascalCase
}

public sealed class DefaultEventNameResolver : IEventNameResolver
{
    private readonly bool _useFullName;
    private readonly bool _includeNamespace;
    private readonly NameCase _case;
    private readonly string? _prefix;
    private readonly Dictionary<Type, (string name, int version)> _overrides = new();

    public DefaultEventNameResolver(bool useFullName = false, bool includeNamespace = false, NameCase @case = NameCase.KebabCase, string? prefix = null)
    {
        _useFullName = useFullName;
        _includeNamespace = includeNamespace;
        _case = @case;
        _prefix = string.IsNullOrWhiteSpace(prefix) ? null : prefix.Trim('.');
    }

    public void Map(Type type, string name, int version = 1) => _overrides[type] = (name, version);

    public string GetName(Type eventType)
    {
        if (_overrides.TryGetValue(eventType, out var o)) return ApplyPrefix(o.name);

        var name = _useFullName
            ? (eventType.FullName ?? eventType.Name)
            : (_includeNamespace ? (eventType.FullName ?? eventType.Name) : eventType.Name);

        // If not including namespace, cut it
        if (!_includeNamespace && name.Contains('.'))
            name = eventType.Name;

        name = _case switch
        {
            NameCase.KebabCase => ToKebabCase(name),
            NameCase.CamelCase => ToCamelCase(name),
            NameCase.PascalCase => ToPascalCase(name),
            _ => name
        };

        return ApplyPrefix(name);
    }

    public int GetVersion(Type eventType)
        => _overrides.TryGetValue(eventType, out var o) ? o.version : 1;

    private string ApplyPrefix(string name) => _prefix is null ? name : $"{_prefix}.{name}";

    private static string ToKebabCase(string s)
    {
        Span<char> buffer = stackalloc char[s.Length * 2];
        int j = 0;
        for (int i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (char.IsLower(s[i - 1]) || (i + 1 < s.Length && char.IsLower(s[i + 1]))))
                    buffer[j++] = '-';
                buffer[j++] = char.ToLowerInvariant(c);
            }
            else if (c == '.')
            {
                buffer[j++] = '.'; // keep namespace separator
            }
            else
            {
                buffer[j++] = c;
            }
        }
        return new string(buffer[..j]);
    }

    private static string ToCamelCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (char.IsLower(s[0])) return s;
        return char.ToLowerInvariant(s[0]) + s[1..];
    }

    private static string ToPascalCase(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpperInvariant(s[0]) + s[1..];
    }
}
