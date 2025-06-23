using System.Data;

namespace Softalleys.Utilities.Extensions;

/// <summary>
/// Provides extension methods for converting a <see cref="DataTable"/> to a <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Converts the contents of a <see cref="DataTable"/> to a <see cref="Dictionary{TKey, TValue}"/>.
    /// Each column name becomes a key, and the corresponding value is taken from the first row.
    /// If the value is <see cref="DBNull"/>, it is converted to <c>null</c>.
    /// </summary>
    /// <param name="source">The <see cref="DataTable"/> to convert.</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> where each key is a column name and each value is the value from the first row,
    /// or <c>null</c> if the value is <see cref="DBNull"/>.
    /// </returns>
    public static Dictionary<string, object?> ToDictionary(this DataTable source)
    {
        var result = new Dictionary<string, object?>();

        foreach (DataRow row in source.Rows)
        {
            foreach (DataColumn column in source.Columns)
            {
                var key = column.ColumnName;
                var value = row[column];

                if (value is DBNull)
                {
                    result[key] = null;
                }
                else
                {
                    result[key] = value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Asynchronously converts the contents of a <see cref="DataTable"/> to a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="source">The <see cref="DataTable"/> to convert.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Dictionary{TKey, TValue}"/>
    /// where each key is a column name and each value is the value from the first row, or <c>null</c> if the value is <see cref="DBNull"/>.
    /// </returns>
    public static async ValueTask<Dictionary<string, object?>> ToDictionaryAsync(this DataTable source, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ToDictionary(source), cancellationToken);
    }
}