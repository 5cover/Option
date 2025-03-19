using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Scover.Options;

/// <summary>
/// Represents an optional value that can either have a value or be empty.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public interface Option<out T>
{
    /// <summary>
    /// Indicates whether the option contains a value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    bool HasValue { get; }

    /// <summary>
    /// Gets the value of the option if it exists.
    /// </summary>
    T? Value { get; }
}

/// <summary>
/// Represents an optional value that can either have a value or contain an error.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
/// <typeparam name="TError">The type of the potential error.</typeparam>
public interface Option<out T, out TError>
{
    /// <summary>
    /// Gets the error associated with the option if no value is present.
    /// </summary>
    TError? Error { get; }

    /// <summary>
    /// Indicates whether the option contains a value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    bool HasValue { get; }

    /// <summary>
    /// Gets the value of the option if it exists.
    /// </summary>
    T? Value { get; }
}

/// <summary>
/// A value type representing an optional value without an error type.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
public readonly record struct ValueOption<T> : Option<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueOption{T}"/> struct.
    /// </summary>
    /// <param name="hasValue">Indicates whether the option has a value.</param>
    /// <param name="value">The optional value.</param>
    public ValueOption(bool hasValue, T? value)
    {
        HasValue = hasValue;
        Value = value;
    }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; }

    /// <inheritdoc/>
    public T? Value { get; }

    /// <summary>
    /// Implicitly converts a value to a <see cref="ValueOption{T}"/> with a value.
    /// </summary>
    public static implicit operator ValueOption<T>(T value) => new(true, value);
}

/// <summary>
/// A value type representing an optional value with an associated error type.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
/// <typeparam name="TError">The type of the potential error.</typeparam>
public readonly record struct ValueOption<T, TError> : Option<T, TError>
{
    /// <summary>
    /// Prevents direct construction without parameters.
    /// </summary>
    public ValueOption() => Debug.Fail("Can't construct without parameters");

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueOption{T, TError}"/> struct.
    /// </summary>
    /// <param name="hasValue">Indicates whether the option has a value.</param>
    /// <param name="value">The optional value.</param>
    /// <param name="error">The optional error.</param>
    public ValueOption(bool hasValue, T? value, TError? error)
    {
        HasValue = hasValue;
        Value = value;
        Error = error;
    }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(HasValue))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool HasValue { get; }

    /// <inheritdoc/>
    public T? Value { get; }

    /// <inheritdoc/>
    public TError? Error { get; }

    /// <summary>
    /// Implicitly converts a value to a <see cref="ValueOption{T, TError}"/> with a value.
    /// </summary>
    public static implicit operator ValueOption<T, TError>(T value) => new(true, value, default);

    /// <summary>
    /// Implicitly converts an error to a <see cref="ValueOption{T, TError}"/> without a value.
    /// </summary>
    public static implicit operator ValueOption<T, TError>(TError error) => new(false, default, error);
}

/// <summary>
/// Provides utility methods for working with <see cref="Option{T}"/> and <see cref="ValueOption{T}"/> types.
/// </summary>

public static class Option
{
    /// <summary>
    /// Transforms an option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting option's value.</typeparam>
    /// <param name="option">The source option to bind.</param>
    /// <param name="transform">A function to transform the option's value into a new option.</param>
    /// <returns>A new option resulting from the transformation, or None if the original option was empty.</returns>
    public static Option<TResult> Bind<T, TResult>(this Option<T> option, Func<T, Option<TResult>> transform)
        => option.HasValue ? transform(option.Value) : None<TResult>();

    /// <summary>
    /// Transforms an option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting option's value.</typeparam>
    /// <param name="option">The source option to bind.</param>
    /// <param name="transform">A function to transform the option's value into a new value option.</param>
    /// <returns>A new value option resulting from the transformation, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Bind<T, TResult>(this Option<T> option, Func<T, ValueOption<TResult>> transform)
        => option.HasValue ? transform(option.Value) : None<TResult>();

    /// <summary>
    /// Transforms a tuple option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting option's value.</typeparam>
    /// <param name="option">The source tuple option to bind.</param>
    /// <param name="transform">A function to transform the tuple's elements into a new option.</param>
    /// <returns>A new option resulting from the transformation, or None if the original option was empty.</returns>
    public static Option<TResult> Bind<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, Option<TResult>> transform)
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : None<TResult>();

    /// <summary>
    /// Transforms a tuple option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source tuple option to bind.</param>
    /// <param name="transform">A function to transform the tuple's elements into a new value option.</param>
    /// <returns>A new value option resulting from the transformation, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Bind<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, ValueOption<TResult>> transform)
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : None<TResult>();

    /// <summary>
    /// Transforms an option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source option to bind.</param>
    /// <param name="transform">A function to transform the option's value into a new option.</param>
    /// <returns>A new option resulting from the transformation, or None with the original error if the option was empty.</returns>
    public static Option<TResult, TError> Bind<T, TResult, TError>(
        this Option<T, TError> option,
        Func<T, Option<TResult, TError>> transform
    )
        => option.HasValue ? transform(option.Value) : None<TResult, TError>(option.Error);

    /// <summary>
    /// Transforms an option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source option to bind.</param>
    /// <param name="transform">A function to transform the option's value into a new value option.</param>
    /// <returns>A new value option resulting from the transformation, or None with the original error if the option was empty.</returns>
    public static ValueOption<TResult, TError> Bind<T, TResult, TError>(
        this Option<T, TError> option,
        Func<T, ValueOption<TResult, TError>> transform
    )
        => option.HasValue ? transform(option.Value) : None<TResult, TError>(option.Error);

    /// <summary>
    /// Transforms a tuple option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source tuple option to bind.</param>
    /// <param name="transform">A function to transform the tuple's elements into a new option.</param>
    /// <returns>A new option resulting from the transformation, or None with the original error if the option was empty.</returns>
    public static Option<TResult, TError> Bind<T1, T2, TResult, TError>(
        this Option<(T1, T2), TError> option,
        Func<T1, T2, Option<TResult, TError>> transform
    )
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : None<TResult, TError>(option.Error);

    /// <summary>
    /// Transforms a tuple option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source tuple option to bind.</param>
    /// <param name="transform">A function to transform the tuple's elements into a new value option.</param>
    /// <returns>A new value option resulting from the transformation, or None with the original error if the option was empty.</returns>
    public static ValueOption<TResult, TError> Bind<T1, T2, TResult, TError>(
        this Option<(T1, T2), TError> option,
        Func<T1, T2, ValueOption<TResult, TError>> transform
    )
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : None<TResult, TError>(option.Error);

    /// <summary>
    /// Removes the error from an option, optionally performing an action with the error.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source option to drop the error from.</param>
    /// <param name="actionWithError">Optional action to perform with the error if the option is empty.</param>
    /// <returns>A new value option without the error.</returns>
    public static ValueOption<T> DropError<T, TError>(this Option<T, TError> option, Action<TError>? actionWithError = null)
    {
        if (!option.HasValue)
        {
            actionWithError?.Invoke(option.Error);
        }
        return new(option.HasValue, option.Value);
    }

    /// <summary>
    /// Transforms an option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source option to map.</param>
    /// <param name="transform">A function to transform the option's value.</param>
    /// <returns>A new value option with the transformed value, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> transform)
        => option.HasValue ? transform(option.Value).Some() : default;

    /// <summary>
    /// Transforms a tuple option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source tuple option to map.</param>
    /// <param name="transform">A function to transform the tuple's elements.</param>
    /// <returns>A new value option with the transformed value, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Map<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, TResult> transform)
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2).Some() : default;

    /// <summary>
    /// Transforms a three-element tuple option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="T3">The type of the third tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source tuple option to map.</param>
    /// <param name="transform">A function to transform the tuple's elements.</param>
    /// <returns>A new value option with the transformed value, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Map<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, TResult> transform)
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2, option.Value.Item3).Some() : default;

    /// <summary>
    /// Transforms an option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source option to map.</param>
    /// <param name="transform">A function to transform the option's value.</param>
    /// <returns>A new value option with the transformed value, or None with the original error if the option was empty.</returns>
    public static ValueOption<TResult, TError> Map<T, TResult, TError>(this Option<T, TError> option, Func<T, TResult> transform)
        => option.HasValue ? transform(option.Value) : option.Error;

    /// <summary>
    /// Transforms a tuple option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source tuple option to map.</param>
    /// <param name="transform">A function to transform the tuple's elements.</param>
    /// <returns>A new value option with the transformed value, or None with the original error if the option was empty.</returns>
    public static ValueOption<TResult, TError> Map<T1, T2, TResult, TError>(this Option<(T1, T2), TError> option, Func<T1, T2, TResult> transform)
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : option.Error;

    /// <summary>
    /// Transforms a three-element tuple option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="T3">The type of the third tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source tuple option to map.</param>
    /// <param name="transform">A function to transform the tuple's elements.</param>
    /// <returns>A new value option with the transformed value, or None with the original error if the option was empty.</returns>
    public static ValueOption<TResult, TError> Map<T1, T2, T3, TResult, TError>(this Option<(T1, T2, T3), TError> option, Func<T1, T2, T3, TResult> transform)
        => option.HasValue ? transform(option.Value.Item1, option.Value.Item2, option.Value.Item3) : option.Error;

    /// <summary>
    /// Transforms the error of an option to a new error type.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the original error.</typeparam>
    /// <typeparam name="TNewError">The type of the new error.</typeparam>
    /// <param name="option">The source option to map the error.</param>
    /// <param name="transform">A function to transform the error.</param>
    /// <returns>A new value option with the transformed error, or the original value if the option has a value.</returns>
    public static ValueOption<T, TNewError> MapError<T, TError, TNewError>(this Option<T, TError> option, Func<TError, TNewError> transform)
        => option.HasValue ? Some<T, TNewError>(option.Value) : None<T, TNewError>(transform(option.Error));

    /// <summary>
    /// Evaluates the option, returning one of two possible results based on whether it contains a value.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the matching functions.</typeparam>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="some">The function to invoke if the option has a value.</param>
    /// <param name="none">The function to invoke if the option is empty.</param>
    /// <returns>The result of either the <paramref name="some"/> or <paramref name="none"/> function.</returns>

    public static TResult Match<T, TResult>(this Option<T> option, Func<T, TResult> some, Func<TResult> none)
        => option.HasValue ? some(option.Value) : none();

    /// <summary>
    /// Evaluates the tuple option, returning one of two possible results based on whether it contains a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the matching functions.</typeparam>
    /// <param name="option">The tuple option to evaluate.</param>
    /// <param name="some">The function to invoke if the option has a value, using the tuple elements.</param>
    /// <param name="none">The function to invoke if the option is empty.</param>
    /// <returns>The result of either the <paramref name="some"/> or <paramref name="none"/> function.</returns>
    public static TResult Match<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, TResult> some, Func<TResult> none)
        => option.HasValue ? some(option.Value.Item1, option.Value.Item2) : none();

    /// <summary>
    /// Transforms an option based on whether it contains a value or an error, returning the corresponding result.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the matching functions.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="option">The option to evaluate.</param>
    /// <param name="some">The function to invoke if the option has a value.</param>
    /// <param name="none">The function to invoke if the option has an error.</param>
    /// <returns>The result of either the <paramref name="some"/> or <paramref name="none"/> function.</returns>

    public static TResult Match<T, TResult, TError>(this Option<T, TError> option, Func<T, TResult> some, Func<TError, TResult> none)
        => option.HasValue ? some(option.Value) : none(option.Error);

    /// <summary>
    /// Evaluates the tuple option with error handling, returning one of two possible results based on whether it contains a value or an error.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the matching functions.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="option">The tuple option to evaluate.</param>
    /// <param name="some">The function to invoke if the option has a value, using the tuple elements.</param>
    /// <param name="none">The function to invoke if the option has an error.</param>
    /// <returns>The result of either the <paramref name="some"/> or <paramref name="none"/> function.</returns>

    public static TResult Match<T1, T2, TResult, TError>(this Option<(T1, T2), TError> option, Func<T1, T2, TResult> some, Func<TError, TResult> none)
        => option.HasValue ? some(option.Value.Item1, option.Value.Item2) : none(option.Error);

    /// <summary>
    /// Filters the option based on a predicate, returning the original option if the predicate is true, or an empty option otherwise.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">A function that tests the option's value.</param>
    /// <returns>The original option if it has a value and satisfies the predicate, otherwise an empty option.</returns>
    public static Option<T> Must<T>(this Option<T> option, Predicate<T> predicate)
    => option.HasValue && predicate(option.Value) ? option : None<T>();

    /// <summary>
    /// Filters the option based on a predicate, returning the original option if the predicate is true, or an empty option with a custom error if the predicate is false.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">A function that tests the option's value.</param>
    /// <param name="falseError">A function that generates an error when the predicate is false.</param>
    /// <returns>The original option if it has a value and satisfies the predicate, otherwise an empty option with the generated error.</returns>
    public static Option<T, TError> Must<T, TError>(this Option<T, TError> option, Predicate<T> predicate, Func<T, TError> falseError)
    => option.HasValue
        ? predicate(option.Value)
            ? option
            : None<T, TError>(falseError(option.Value))
        : option;

    /// <summary>
    /// Creates an empty <see cref="ValueOption{T}"/> with no value.
    /// </summary>
    /// <typeparam name="T">The type of the potential value.</typeparam>
    /// <returns>A <see cref="ValueOption{T}"/> representing the absence of a value.</returns>
    public static ValueOption<T> None<T>() => new(false, default);

    /// <summary>
    /// Creates an empty <see cref="ValueOption{T, TError}"/> containing the provided error.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="error">The error to include in the option.</param>
    public static ValueOption<T, TError> None<T, TError>(this TError error) => new(false, default, error);

    /// <summary>
    /// Returns the option itself if it has a value; otherwise, returns the specified fallback option.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="fallback">The fallback option.</param>
    public static Option<T> Or<T>(this Option<T> option, Option<T> fallback)
        => option.HasValue ? option : fallback;

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, returns the specified fallback value option.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">The fallback value option.</param>
    public static ValueOption<T> Or<T>(this ValueOption<T> option, ValueOption<T> fallback)
        => option.HasValue ? option : fallback;

    /// <summary>
    /// Returns the option itself if it has a value; otherwise, evaluates and returns the fallback option provided by a function.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="fallback">A function that provides a fallback option.</param>
    public static Option<T> Or<T>(this Option<T> option, Func<Option<T>> fallback)
        => option.HasValue ? option : fallback();

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, evaluates and returns the fallback value option provided by a function.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">A function that provides a fallback value option.</param>
    public static ValueOption<T> Or<T>(this ValueOption<T> option, Func<ValueOption<T>> fallback)
        => option.HasValue ? option : fallback();

    /// <summary>
    /// Returns the option itself if it has a value; otherwise, returns the specified fallback option with error.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="fallback">The fallback option with error.</param>
    public static Option<T, TError> Or<T, TError>(this Option<T, TError> option, Option<T, TError> fallback)
        => option.HasValue ? option : fallback;

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, returns the specified fallback value option with error.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">The fallback value option with error.</param>
    public static ValueOption<T, TError> Or<T, TError>(this ValueOption<T, TError> option, ValueOption<T, TError> fallback)
        => option.HasValue ? option : fallback;

    /// <summary>
    /// Returns the option itself if it has a value; otherwise, evaluates and returns the fallback option with error provided by a function.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="fallback">A function providing a fallback option with error.</param>
    public static Option<T, TError> Or<T, TError>(this Option<T, TError> option, Func<Option<T, TError>> fallback)
        => option.HasValue ? option : fallback();

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, evaluates and returns the fallback value option with error provided by a function.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">A function providing a fallback value option with error.</param>
    public static ValueOption<T, TError> Or<T, TError>(this ValueOption<T, TError> option, Func<ValueOption<T, TError>> fallback)
        => option.HasValue ? option : fallback();

    /// <summary>
    /// Returns a value option with the specified error if the original option has no value; otherwise, returns a value option with the original option's value.
    /// </summary>
    /// <typeparam name="T">The type of the original option's value.</typeparam>
    /// <typeparam name="TError">The type of the error to use when the original option has no value.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="error">The error to use when the original option has no value.</param>
    /// <returns>A value option containing either the original value or the specified error.</returns>
    public static ValueOption<T, TError> OrWithError<T, TError>(this Option<T> option, TError error)
    => option.HasValue ? option.Value : error;

    /// <summary>
    /// Creates a new <see cref="ValueOption{T}"/> with a value, indicating the option contains a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to be wrapped in the option.</param>
    /// <returns>A <see cref="ValueOption{T}"/> containing the specified value.</returns>
    public static ValueOption<T> Some<T>(this T value) => new(true, value);

    /// <summary>
    /// Creates a new <see cref="ValueOption{T, TError}"/> with a value and default error, indicating the option contains a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TError">The type of the potential error.</typeparam>
    /// <param name="value">The value to be wrapped in the option.</param>
    /// <returns>A <see cref="ValueOption{T, TError}"/> containing the specified value and a default error.</returns>
    public static ValueOption<T, TError> Some<T, TError>(this T value)
    => new(true, value, default);

    /// <summary>
    /// Attempts to cast the given object to the specified type and wrap it in a <see cref="ValueOption{T}"/>.
    /// </summary>
    /// <typeparam name="T">The target type to cast to.</typeparam>
    /// <param name="obj">The object to attempt to cast.</param>
    /// <returns>
    /// A <see cref="ValueOption{T}"/> containing the casted value if the cast is successful; otherwise, an empty <see cref="ValueOption{T}"/>.
    /// </returns>
    public static ValueOption<T> SomeAs<T>(this object obj)
        => obj is T t ? t.Some() : None<T>();

    /// <summary>
    /// Performs an action on the option's value if it exists, or an alternative action if the option is empty.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="some">An optional action to perform when the option has a value.</param>
    /// <param name="none">An optional action to perform when the option is empty.</param>
    /// <returns>The original option, allowing for method chaining.</returns>
    public static Option<T> Tap<T>(this Option<T> option, Action<T>? some = null, Action? none = null)
    {
        if (option.HasValue)
        {
            some?.Invoke(option.Value);
        }
        else
        {
            none?.Invoke();
        }
        return option;
    }

    /// <summary>
    /// Performs an action on the option's value if it exists, or an alternative action if the option is empty.
    /// </summary>
    /// <typeparam name="T1">The first type of the tuple value.</typeparam>
    /// <typeparam name="T2">The second type of the tuple value.</typeparam>
    /// <param name="option">The original option containing a tuple value.</param>
    /// <param name="some">An optional action to perform when the option has a value, receiving the tuple's two elements.</param>
    /// <param name="none">An optional action to perform when the option is empty.</param>
    /// <returns>The original option, allowing for method chaining.</returns>
    public static Option<(T1, T2)> Tap<T1, T2>(this Option<(T1, T2)> option, Action<T1, T2>? some = null, Action? none = null)
    {
        if (option.HasValue)
        {
            some?.Invoke(option.Value.Item1, option.Value.Item2);
        }
        else
        {
            none?.Invoke();
        }
        return option;
    }

    /// <summary>
    /// Performs an action on the option's value if it exists, or an alternative action if the option is empty.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original option with an error type.</param>
    /// <param name="some">An optional action to perform when the option has a value.</param>
    /// <param name="none">An optional action to perform when the option is empty, receiving the error.</param>
    /// <returns>The original option, allowing for method chaining.</returns>
    public static Option<T, TError> Tap<T, TError>(this Option<T, TError> option, Action<T>? some = null, Action<TError>? none = null)
    {
        if (option.HasValue)
        {
            some?.Invoke(option.Value);
        }
        else
        {
            none?.Invoke(option.Error);
        }
        return option;
    }

    /// <summary>
    /// Performs an action on the option's value if it exists, or an alternative action if the option is empty.
    /// </summary>
    /// <typeparam name="T1">The first type of the tuple value.</typeparam>
    /// <typeparam name="T2">The second type of the tuple value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original option with an error type.</param>
    /// <param name="some">An optional action to perform when the option has a value.</param>
    /// <param name="none">An optional action to perform when the option is empty, receiving the error.</param>
    /// <returns>The original option, allowing for method chaining.</returns>

    public static Option<(T1, T2), TError> Tap<T1, T2, TError>(this Option<(T1, T2), TError> option, Action<T1, T2>? some = null, Action<TError>? none = null)
    {
        if (option.HasValue)
        {
            some?.Invoke(option.Value.Item1, option.Value.Item2);
        }
        else
        {
            none?.Invoke(option.Error);
        }
        return option;
    }

    /// <summary>
    /// Unwraps the Option, returning its value if present, or throwing an <see cref="InvalidOperationException"/> if no value exists.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The Option to unwrap.</param>
    /// <returns>The value contained in the Option.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Option has no value.</exception>
    public static T Unwrap<T, TError>(this Option<T, TError> option)
    => option.HasValue ? option.Value : throw new InvalidOperationException("Option has no value");

    /// <summary>
    /// Unwraps the Option, returning its value if present, or throwing an <see cref="InvalidOperationException"/> if no value exists.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The Option to unwrap.</param>
    /// <returns>The value contained in the Option.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Option has no value.</exception>
    public static T Unwrap<T>(this Option<T> option)
    => option.HasValue ? option.Value : throw new InvalidOperationException("Option has no value");

    /// <summary>
    /// Returns the value of the Option if it has a value, otherwise returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The Option to retrieve the value from.</param>
    /// <param name="defaultValue">The value to return if the Option has no value.</param>
    /// <returns>The option's value or the specified default value.</returns>
    public static T ValueOr<T>(this Option<T> option, T defaultValue)
    => option.HasValue ? option.Value : defaultValue;

    /// <summary>
    /// Returns the value of the Option if it has a value, otherwise returns the specified default value.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The Option to retrieve the value from.</param>
    /// <param name="defaultValue">The value to return if the Option has no value.</param>
    /// <returns>The option's value or the specified default value.</returns>
    public static T ValueOr<T, TError>(this Option<T, TError> option, T defaultValue)
        => option.HasValue ? option.Value : defaultValue;

    /// <summary>
    /// Returns the value of the Option if it has a value, otherwise returns the result of invoking the provided default value function.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The Option to retrieve the value from.</param>
    /// <param name="defaultValue">A function that provides the default value if the Option has no value.</param>
    /// <returns>The option's value or the result of the default value function.</returns>
    public static T ValueOr<T>(this Option<T> option, Func<T> defaultValue)
    => option.HasValue ? option.Value : defaultValue();

    /// <summary>
    /// Returns the value of the Option if it has a value, otherwise returns the result of invoking the provided default value function.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The Option to retrieve the value from.</param>
    /// <param name="defaultValue">A function that provides the default value if the Option has no value.</param>
    /// <returns>The option's value or the result of the default value function.</returns>
    public static T ValueOr<T, TError>(this Option<T, TError> option, Func<T> defaultValue)
        => option.HasValue ? option.Value : defaultValue();

    /// <summary>
    /// Returns the value of the Option if it has a value, otherwise returns the default value for the type.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The Option to retrieve the value from.</param>
    /// <returns>The option's value or the default value for type T.</returns>
    public static T? ValueOrDefault<T>(this Option<T> option)
    => option.HasValue ? option.Value : default;

    /// <summary>
    /// Returns the value of the Option if it has a value, otherwise returns the default value for the type.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The Option to retrieve the value from.</param>
    /// <returns>The option's value or the default value for type T.</returns>
    public static T? ValueOrDefault<T, TError>(this Option<T, TError> option)
    => option.HasValue ? option.Value : default;

    /// <summary>
    /// Combines two Option instances into a single Option containing a tuple of their values.
    /// </summary>
    /// <typeparam name="T1">The type of the first option's value.</typeparam>
    /// <typeparam name="T2">The type of the second option's value.</typeparam>
    /// <param name="option1">The first Option to combine.</param>
    /// <param name="option2">The second Option to combine.</param>
    /// <returns>
    /// An Option containing a tuple of values if both input Options have values, 
    /// otherwise returns an empty Option.
    /// </returns>
    public static Option<(T1, T2)> Zip<T1, T2>(this Option<T1> option1, Option<T2> option2)
    => option1.HasValue && option2.HasValue
        ? (option1.Value, option2.Value).Some()
        : None<(T1, T2)>();

    /// <summary>
    /// Combines two Option instances with the same error type into a single Option containing a tuple of their values.
    /// </summary>
    /// <typeparam name="T1">The type of the first option's value.</typeparam>
    /// <typeparam name="T2">The type of the second option's value.</typeparam>
    /// <typeparam name="TError">The type of the error for both options.</typeparam>
    /// <param name="option1">The first Option to combine.</param>
    /// <param name="option2">The second Option to combine.</param>
    /// <returns>
    /// An Option containing a tuple of values if both input Options have values, 
    /// otherwise returns an Option with the first encountered error.
    /// </returns>
    public static Option<(T1, T2), TError> Zip<T1, T2, TError>(this Option<T1, TError> option1, Option<T2, TError> option2)
    => option1.HasValue && option2.HasValue
        ? Some<(T1, T2), TError>((option1.Value, option2.Value))
        : None<(T1, T2), TError>(option1.HasValue ? option2.HasValue ? throw new UnreachableException() : option2.Error : option1.Error);
}