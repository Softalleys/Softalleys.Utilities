namespace Softalleys.Utilities.Extensions;

/// <summary>
///     Provides extension methods for working with enumerable collections.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     Returns the enumerable collection or an empty collection if it is null.
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? source)
    {
        return source ?? [];
    }

    /// <summary>
    ///     Traverses up a hierarchy from the given item using the specified parent selector function.
    /// </summary>
    public static IEnumerable<T> TraverseUpwards<T>(this T item, Func<T, T?> parentSelector) where T : class
    {
        for (var current = item; current != null;)
        {
            yield return current;

            var parent = parentSelector(current);

            if (ReferenceEquals(parent, current))
                yield break;

            current = parent;
        }
    }

    /// <summary>
    ///     Flattens a tree structure into a single enumerable collection.
    /// </summary>
    public static IEnumerable<T> FlattenHierarchy<T>(this IEnumerable<T>? source,
        Func<T, IEnumerable<T>> childrenSelector)
    {
        var queue = new Queue<T>();
        queue.EnqueueAll(source);
        return queue.FlattenHierarchyImpl(childrenSelector);
    }

    /// <summary>
    ///     Flattens a tree structure starting from the given root into a single enumerable collection.
    /// </summary>
    public static IEnumerable<T> FlattenHierarchy<T>(this T root, Func<T, IEnumerable<T>> childrenSelector)
    {
        var queue = new Queue<T>();
        queue.Enqueue(root);
        return queue.FlattenHierarchyImpl(childrenSelector);
    }

    private static IEnumerable<T> FlattenHierarchyImpl<T>(this Queue<T> queue, Func<T, IEnumerable<T>> childrenSelector)
    {
        while (queue.Count > 0)
        {
            var item = queue.Dequeue();
            yield return item;
            queue.EnqueueAll(childrenSelector(item));
        }
    }

    /// <summary>
    ///     Enqueues all elements from the given enumerable collection into the queue.
    /// </summary>
    public static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T>? source)
    {
        if (source == null) return;
        foreach (var item in source) queue.Enqueue(item);
    }
}