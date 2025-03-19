﻿using System.Diagnostics.CodeAnalysis;

namespace Scover.Option;

public static class OptionCollectionExtensions
{
    /// <summary>
    /// Transform a list of optionals into an optional containing a list of values.
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
        foreach (var o in options)
        {
            if (o.HasValue)
            {
                values.Add(o.Value);
            }
            else
            {
                errors.Add(o.Error);
            }
        }

        return errors.Count > 0 ? errors : values;
    }

    /// <inheritdoc cref="Sequence{T, TError}(IEnumerable{Option{T, TError}})"/>
    public static ValueOption<IReadOnlyList<T>, IReadOnlyList<TError>> Sequence<T, TError>(this IEnumerable<ValueOption<T, TError>> options)
        => Sequence(options.Cast<Option<T, TError>>());

    /// <summary>
    /// Transform a list of optionals into an optional containing a list of values.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="options">The list of optionals.</param>
    /// <returns>If all the optionals contain values, the result is an optional containing a list of those values. If any of the optionals are empty, the result is an empty optional.</returns>
    public static ValueOption<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Option<T>> options)
    {
        List<T> values = [];

        foreach (var o in options)
        {
            if (o.HasValue)
            {
                values.Add(o.Value);
            }
            else
            {
                return default;
            }
        }

        return values;
    }

    /// <inheritdoc cref="Sequence{T}(IEnumerable{Option{T}})"/>
    public static ValueOption<IReadOnlyList<T>> Sequence<T>(this IEnumerable<ValueOption<T>> options)
        => Sequence(options.Cast<Option<T>>());

    public static ValueOption<T> ElementAtOrNone<T>(this IEnumerable<T> source, int index) where T : notnull
        => source.TryGetAt(index, out var item)
            ? item.Some()
            : default;

    public static ValueOption<T> FirstOrNone<T>(this IEnumerable<T> source)
    {
        switch (source)
        {
            case IReadOnlyList<T> roList:
                {
                    if (roList.Count > 0)
                    {
                        return roList[0];
                    }
                    break;
                }
            case IList<T> list:
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                    break;
                }
            default:
                {
                    using var enumerator = source.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }
                    break;
                }
        }
        return default;
    }

    public static ValueOption<TValue> GetValueOrNone<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        => dictionary.TryGetValue(key, out var v) ? v.Some() : default;

    public static ValueOption<KeyValuePair<TKey, TValue>> GetEntryOrNone<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        => dictionary.TryGetValue(key, out var v) ? KeyValuePair.Create(key, v) : default;

    public static IEnumerable<T> WhereSome<T>(this IEnumerable<Option<T>> source) => source.Where(o => o.HasValue).Select(o => o.Value!);
    public static IEnumerable<T> WhereSome<T>(this IEnumerable<ValueOption<T>> source) => source.Where(o => o.HasValue).Select(o => o.Value!);

    public static bool TryGetAt<T>(this IEnumerable<T> source, int index, [NotNullWhen(true)] out T? item) where T : notnull
    {
        if (index >= 0)
        {
            switch (source)
            {
                case IReadOnlyList<T> rolist:
                    {
                        if (index < rolist.Count)
                        {
                            item = rolist[index];
                            return true;
                        }
                        break;
                    }
                case IList<T> list:
                    {
                        if (index < list.Count)
                        {
                            item = list[index];
                            return true;
                        }
                        break;
                    }
                default:
                    {
                        using var enumerator = source.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            if (index-- == 0)
                            {
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