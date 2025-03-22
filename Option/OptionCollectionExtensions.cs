using System.Diagnostics.CodeAnalysis;

namespace Scover.Options;

/// <summary>
/// Provides extension methods for working with collections of <see cref="Option{T}"/> and <see cref="ValueOption{T}"/>.
/// </summary>
public static class OptionCollectionExtensions
{
    /// <summary>
    /// Transforms a list of optionals into an optional containing a list of values.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="options">The list of optionals.</param>
    /// <returns>If all the optionals contain values, the result is an optional containing a list of those values. If any of the optionals are erroneous, the result is an erroneous optional containing all the errors.</returns>
    public static ValueOption<IReadOnlyList<T>, IReadOnlyList<TError>> Sequence<T, TError>(this IEnumerable<Option<T, TError>> options)
    {
        List<T> values = [];
        List<TError> errors = [];

        // Make sure to only iterate the input sequence once.
        foreach (Option<T, TError> o in options) {
            if (o.HasValue) {
                values.Add(o.Value);
            } else {
                errors.Add(o.Error);
            }
        }

        return errors.Count > 0 ? errors : values;
    }

    /// <inheritdoc cref="Sequence{T, TError}(IEnumerable{Option{T, TError}})"/>
    public static ValueOption<IReadOnlyList<T>, IReadOnlyList<TError>> Sequence<T, TError>(this IEnumerable<ValueOption<T, TError>> options) =>
        Sequence(options.Cast<Option<T, TError>>());

    /// <summary>
    /// Transforms a list of optionals into an optional containing a list of values.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="options">The list of optionals.</param>
    /// <returns>If all the optionals contain values, the result is an optional containing a list of those values. If any of the optionals are empty, the result is an empty optional.</returns>
    public static ValueOption<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        List<T> values = [];

        foreach (Option<T> o in options) {
            if (o.HasValue) {
                values.Add(o.Value);
            } else {
                return default;
            }
        }

        return values;
    }

    /// <inheritdoc cref="Sequence{T}(IEnumerable{Option{T}})"/>
    public static ValueOption<IReadOnlyList<T>> Sequence<T>(this IEnumerable<ValueOption<T>> options) => Sequence(options.Cast<Option<T>>());

    /// <summary>
    /// Returns the element at the specified index as a <see cref="ValueOption{T}"/>, or a default value if the index is out of range.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to retrieve the element from.</param>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>A <see cref="ValueOption{T}"/> containing the element at the specified index, or a default value if the index is out of range.</returns>
    public static ValueOption<T> ElementAtOrNone<T>(this IEnumerable<T> source, int index) where T : notnull => source.TryGetAt(index, out T? item)
        ? item.Some()
        : default;

    /// <summary>
    /// Returns the first element of the sequence as a <see cref="ValueOption{T}"/>, or a default value if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The sequence to retrieve the first element from.</param>
    /// <returns>A <see cref="ValueOption{T}"/> containing the first element, or a default value if the sequence is empty.</returns>
    public static ValueOption<T> FirstOrNone<T>(this IEnumerable<T> source)
    {
        switch (source) {
        case IReadOnlyList<T> roList: {
            if (roList.Count > 0) {
                return roList[0];
            }
            break;
        }
        case IList<T> list: {
            if (list.Count > 0) {
                return list[0];
            }
            break;
        }
        default: {
            using IEnumerator<T> enumerator = source.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }
            break;
        }
        }
        return default;
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key from the dictionary as a
    /// <see
    ///     cref="ValueOption{TValue}"/>
    /// .
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to retrieve the value from.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <returns>A <see cref="ValueOption{TValue}"/> containing the value if the key exists, or a default value if the key is not found.</returns>
    public static ValueOption<TValue> GetValueOrNone<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out TValue? v) ? v.Some() : default;

    /// <summary>
    /// Attempts to retrieve the key-value pair associated with the specified key from the dictionary as a
    /// <see
    ///     cref="ValueOption{KeyValuePair}"/>
    /// .
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the value in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to retrieve the entry from.</param>
    /// <param name="key">The key to look up in the dictionary.</param>
    /// <returns>A <see cref="ValueOption{KeyValuePair}"/> containing the key-value pair if the key exists, or a default value if the key is not found.</returns>
    public static ValueOption<KeyValuePair<TKey, TValue>> GetEntryOrNone<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out TValue? v) ? KeyValuePair.Create(key, v) : default;

    /// <summary>
    /// Filters an enumerable of <see cref="Option{T}"/> to return only the elements with a value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The source enumerable of <see cref="Option{T}"/>.</param>
    /// <returns>An enumerable containing only the non-empty option values.</returns>
    public static IEnumerable<T> WhereSome<T>(this IEnumerable<Option<T>> source) => source.Where(o => o.HasValue).Select(o => o.Value!);

    /// <summary>
    /// Filters an enumerable of <see cref="ValueOption{T}"/> to return only the elements with a value.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The source enumerable of <see cref="ValueOption{T}"/>.</param>
    /// <returns>An enumerable containing only the non-empty value option values.</returns>
    public static IEnumerable<T> WhereSome<T>(this IEnumerable<ValueOption<T>> source) => source.Where(o => o.HasValue).Select(o => o.Value!);

    /// <summary>
    /// Attempts to retrieve the item at the specified index from the source enumerable.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="source">The source enumerable to retrieve the item from.</param>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <param name="item">When this method returns, contains the item at the specified index, if the index is valid; otherwise, the default value for the type.</param>
    /// <returns><see langword="true"/> if the index is valid and an item is found; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetAt<T>(this IEnumerable<T> source, int index, [NotNullWhen(true)] out T? item) where T : notnull
    {
        if (index >= 0) {
            switch (source) {
            case IReadOnlyList<T> rolist: {
                if (index < rolist.Count) {
                    item = rolist[index];
                    return true;
                }
                break;
            }
            case IList<T> list: {
                if (index < list.Count) {
                    item = list[index];
                    return true;
                }
                break;
            }
            default: {
                using IEnumerator<T> enumerator = source.GetEnumerator();
                while (enumerator.MoveNext()) {
                    if (index-- == 0) {
                        item = enumerator.Current;
                        return true;
                    }
                }
                break;
            }
            }
        }
        item = default;
        return false;
    }
}
