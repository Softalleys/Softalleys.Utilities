namespace Softalleys.Utilities.Events.Distributed.Types;

public interface IEventTypeRegistry
{
    bool TryGetType(string eventName, int version, out Type? type);
    void Map(Type type, string eventName, int version = 1);
}

public sealed class DefaultEventTypeRegistry : IEventTypeRegistry
{
    private readonly Dictionary<(string name, int version), Type> _map = new(StringTupleComparer.OrdinalIgnoreCaseWithVersion);

    public bool TryGetType(string eventName, int version, out Type? type)
        => _map.TryGetValue((eventName, version), out type);

    public void Map(Type type, string eventName, int version = 1)
        => _map[(eventName, version)] = type;

    private sealed class StringTupleComparer : IEqualityComparer<(string name, int version)>
    {
        public static readonly StringTupleComparer OrdinalIgnoreCaseWithVersion = new();
        public bool Equals((string name, int version) x, (string name, int version) y)
            => x.version == y.version && string.Equals(x.name, y.name, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode((string name, int version) obj)
            => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(obj.name), obj.version);
    }
}
