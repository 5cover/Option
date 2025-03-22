using System.Diagnostics.CodeAnalysis;

namespace Scover.Options;

public static partial class Option
{
    #region OrWithError

    /// <summary>
    /// Returns a value option with the specified error if the original option has no value; otherwise, returns a value option with the original option's value.
    /// </summary>
    /// <typeparam name="T">The type of the original option's value.</typeparam>
    /// <typeparam name="TError">The type of the error to use when the original option has no value.</typeparam>
    /// <param name="option">The original option.</param>
    /// <param name="error">The error to use when the original option has no value.</param>
    /// <returns>A value option containing either the original value or the specified error.</returns>
    public static ValueOption<T, TError> OrWithError<T, TError>(this Option<T> option, TError error) => option.HasValue ? option.Value : error;

    #endregion OrWithError

    #region DropError

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
        if (!option.HasValue) actionWithError?.Invoke(option.Error);
        return new(option.HasValue, option.Value);
    }

    #endregion DropError

    #region SomeAs

    /// <summary>
    /// Attempts to cast the given object to the specified type and wrap it in a <see cref="ValueOption{T}"/>.
    /// </summary>
    /// <typeparam name="T">The target type to cast to.</typeparam>
    /// <param name="obj">The object to attempt to cast.</param>
    /// <returns>
    /// A <see cref="ValueOption{T}"/> containing the cast value if the cast is successful; otherwise, an empty <see cref="ValueOption{T}"/>.
    /// </returns>
    public static ValueOption<T> SomeAs<T>(this object obj) => obj is T t ? t.Some() : None<T>();

    #endregion SomeAs

    #region Bind

    /// <summary>
    /// Transforms an option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting option's value.</typeparam>
    /// <param name="option">The source option to bind.</param>
    /// <param name="transform">A function to transform the option's value into a new value option.</param>
    /// <returns>A new value option resulting from the transformation, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Bind<T, TResult>(this Option<T> option, Func<T, ValueOption<TResult>> transform) =>
        option.HasValue ? transform(option.Value) : None<TResult>();

    /// <summary>
    /// Transforms a tuple option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source tuple option to bind.</param>
    /// <param name="transform">A function to transform the tuple's elements into a new value option.</param>
    /// <returns>A new value option resulting from the transformation, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Bind<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, ValueOption<TResult>> transform) =>
        option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : None<TResult>();

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
    ) => option.HasValue ? transform(option.Value) : None<TResult, TError>(option.Error);

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
    ) => option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : None<TResult, TError>(option.Error);

    #endregion Bind

    #region Must

    /// <summary>
    /// Filters the value option based on a predicate, returning the original value option if the predicate is true, or an empty value option otherwise.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">A function that tests the option's value.</param>
    /// <returns>The original option if it has a value and satisfies the predicate, otherwise an empty value option.</returns>
    public static ValueOption<T> Must<T>(this ValueOption<T> option, Predicate<T> predicate) => option.HasValue && predicate(option.Value) ? option : None<T>();

    /// <summary>
    /// Filters the value option based on a predicate, returning the original value option if the predicate is true, or an empty value option with a custom error if the predicate is false.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">A function that tests the option's value.</param>
    /// <param name="falseError">A function that generates an error when the predicate is false.</param>
    /// <returns>The original option if it has a value and satisfies the predicate, otherwise an empty value option with the generated error.</returns>
    public static ValueOption<T, TError> Must<T, TError>(this ValueOption<T, TError> option, Predicate<T> predicate, Func<T, TError> falseError) =>
        option.HasValue
            ? predicate(option.Value)
                ? option
                : None<T, TError>(falseError(option.Value))
            : option;

    #endregion Must

    #region Or

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, returns the specified fallback value option.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">The fallback value option.</param>
    public static ValueOption<T> Or<T>(this ValueOption<T> option, ValueOption<T> fallback) => option.HasValue ? option : fallback;

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, evaluates and returns the fallback value option provided by a function.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">A function that provides a fallback value option.</param>
    public static ValueOption<T> Or<T>(this ValueOption<T> option, Func<ValueOption<T>> fallback) => option.HasValue ? option : fallback();

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, returns the specified fallback value option with error.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">The fallback value option with error.</param>
    public static ValueOption<T, TError> Or<T, TError>(this ValueOption<T, TError> option, ValueOption<T, TError> fallback) =>
        option.HasValue ? option : fallback;

    /// <summary>
    /// Returns the value option itself if it has a value; otherwise, evaluates and returns the fallback value option with error provided by a function.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="fallback">A function providing a fallback value option with error.</param>
    public static ValueOption<T, TError> Or<T, TError>(this ValueOption<T, TError> option, Func<ValueOption<T, TError>> fallback) =>
        option.HasValue ? option : fallback();

    #endregion Or

    #region Tap

    /// <summary>
    /// Performs an action on the value option's value if it exists, or an alternative action if the value option is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <param name="option">The original value option.</param>
    /// <param name="some">An optional action to perform when the value option has a value.</param>
    /// <param name="none">An optional action to perform when the value option is empty.</param>
    /// <returns>The original value option, allowing for method chaining.</returns>
    public static ValueOption<T> Tap<T>(this ValueOption<T> option, Action<T>? some = null, Action? none = null)
    {
        if (option.HasValue) {
            some?.Invoke(option.Value);
        } else {
            none?.Invoke();
        }
        return option;
    }

    /// <summary>
    /// Performs an action on the value option's value if it exists, or an alternative action if the value option is empty.
    /// </summary>
    /// <typeparam name="T1">The first type of the tuple value.</typeparam>
    /// <typeparam name="T2">The second type of the tuple value.</typeparam>
    /// <param name="option">The original value option containing a tuple value.</param>
    /// <param name="some">An optional action to perform when the value option has a value, receiving the tuple's two elements.</param>
    /// <param name="none">An optional action to perform when the value option is empty.</param>
    /// <returns>The original value option, allowing for method chaining.</returns>
    public static ValueOption<(T1, T2)> Tap<T1, T2>(this ValueOption<(T1, T2)> option, Action<T1, T2>? some = null, Action? none = null)
    {
        if (option.HasValue) {
            some?.Invoke(option.Value.Item1, option.Value.Item2);
        } else {
            none?.Invoke();
        }
        return option;
    }

    /// <summary>
    /// Performs an action on the value option's value if it exists, or an alternative action if the value option is empty.
    /// </summary>
    /// <typeparam name="T">The type of the value option's value.</typeparam>
    /// <typeparam name="TError">The type of the value option's error.</typeparam>
    /// <param name="option">The original value option with an error type.</param>
    /// <param name="some">An optional action to perform when the value option has a value.</param>
    /// <param name="none">An optional action to perform when the value option is empty, receiving the error.</param>
    /// <returns>The original value option, allowing for method chaining.</returns>
    public static ValueOption<T, TError> Tap<T, TError>(this ValueOption<T, TError> option, Action<T>? some = null, Action<TError>? none = null)
    {
        if (option.HasValue) {
            some?.Invoke(option.Value);
        } else {
            none?.Invoke(option.Error);
        }
        return option;
    }

    /// <summary>
    /// Performs an action on the value option's value if it exists, or an alternative action if the value option is empty.
    /// </summary>
    /// <typeparam name="T1">The first type of the tuple value.</typeparam>
    /// <typeparam name="T2">The second type of the tuple value.</typeparam>
    /// <typeparam name="TError">The type of the value option's error.</typeparam>
    /// <param name="option">The original value option with an error type.</param>
    /// <param name="some">An optional action to perform when the value option has a value.</param>
    /// <param name="none">An optional action to perform when the value option is empty, receiving the error.</param>
    /// <returns>The original value option, allowing for method chaining.</returns>
    public static ValueOption<(T1, T2), TError> Tap<T1, T2, TError>(
        this ValueOption<(T1, T2), TError> option,
        Action<T1, T2>? some = null,
        Action<TError>? none = null
    )
    {
        if (option.HasValue) {
            some?.Invoke(option.Value.Item1, option.Value.Item2);
        } else {
            none?.Invoke(option.Error);
        }
        return option;
    }

    #endregion Tap

    #region Map

    /// <summary>
    /// Transforms an option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source option to map.</param>
    /// <param name="transform">A function to transform the option's value.</param>
    /// <returns>A new value option with the transformed value, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Map<T, TResult>(this Option<T> option, Func<T, TResult> transform) =>
        option.HasValue ? transform(option.Value).Some() : default;

    /// <summary>
    /// Transforms a tuple option by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T1">The type of the first tuple element.</typeparam>
    /// <typeparam name="T2">The type of the second tuple element.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <param name="option">The source tuple option to map.</param>
    /// <param name="transform">A function to transform the tuple's elements.</param>
    /// <returns>A new value option with the transformed value, or None if the original option was empty.</returns>
    public static ValueOption<TResult> Map<T1, T2, TResult>(this Option<(T1, T2)> option, Func<T1, T2, TResult> transform) =>
        option.HasValue ? transform(option.Value.Item1, option.Value.Item2).Some() : default;

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
    public static ValueOption<TResult> Map<T1, T2, T3, TResult>(this Option<(T1, T2, T3)> option, Func<T1, T2, T3, TResult> transform) =>
        option.HasValue ? transform(option.Value.Item1, option.Value.Item2, option.Value.Item3).Some() : default;

    /// <summary>
    /// Transforms an option with an error by applying a transformation function if the option has a value.
    /// </summary>
    /// <typeparam name="T">The type of the source option's value.</typeparam>
    /// <typeparam name="TResult">The type of the resulting value option's value.</typeparam>
    /// <typeparam name="TError">The type of the option's error.</typeparam>
    /// <param name="option">The source option to map.</param>
    /// <param name="transform">A function to transform the option's value.</param>
    /// <returns>A new value option with the transformed value, or None with the original error if the option was empty.</returns>
    public static ValueOption<TResult, TError> Map<T, TResult, TError>(this Option<T, TError> option, Func<T, TResult> transform) =>
        option.HasValue ? transform(option.Value) : option.Error;

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
    public static ValueOption<TResult, TError> Map<T1, T2, TResult, TError>(this Option<(T1, T2), TError> option, Func<T1, T2, TResult> transform) =>
        option.HasValue ? transform(option.Value.Item1, option.Value.Item2) : option.Error;

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
    public static ValueOption<TResult, TError>
        Map<T1, T2, T3, TResult, TError>(this Option<(T1, T2, T3), TError> option, Func<T1, T2, T3, TResult> transform) =>
        option.HasValue ? transform(option.Value.Item1, option.Value.Item2, option.Value.Item3) : option.Error;

    /// <summary>
    /// Transforms the error of an option to a new error type.
    /// </summary>
    /// <typeparam name="T">The type of the option's value.</typeparam>
    /// <typeparam name="TError">The type of the original error.</typeparam>
    /// <typeparam name="TNewError">The type of the new error.</typeparam>
    /// <param name="option">The source option to map the error.</param>
    /// <param name="transform">A function to transform the error.</param>
    /// <returns>A new value option with the transformed error, or the original value if the option has a value.</returns>
    public static ValueOption<T, TNewError> MapError<T, TError, TNewError>(this Option<T, TError> option, Func<TError, TNewError> transform) =>
        option.HasValue ? Some<T, TNewError>(option.Value) : None<T, TNewError>(transform(option.Error));

    #endregion Map

    #region None

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

    #endregion None

    #region Some

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
    public static ValueOption<T, TError> Some<T, TError>(this T value) => new(true, value, default);

    #endregion Some
}

/// <inheritdoc cref="Option{T}"/>
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

/// <inheritdoc cref="Option{T, TError}"/>
/// <summary>
/// A value type representing an optional value with an associated error type.
/// </summary>
/// <typeparam name="T">The type of the optional value.</typeparam>
/// <typeparam name="TError">The type of the potential error.</typeparam>
public readonly record struct ValueOption<T, TError> : Option<T, TError>
{
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
    [MemberNotNullWhen(true, nameof(Value))]
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
