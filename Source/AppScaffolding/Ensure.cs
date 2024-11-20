using System;

namespace AppScaffolding;

/// <summary>
/// A helper class for ensuring parameter conditions.
/// Yoinked from https://github.com/CoryCharlton/CCSWE.Core/blob/master/src/Core/Ensure.cs
/// </summary>
public static class Ensure
{
    private static Exception GetException<TException>(string name, string message) where TException : Exception, new()
    {
        Exception exception = (TException)Activator.CreateInstance(typeof(TException), message);

        if (exception is ArgumentNullException)
        {
            return new ArgumentNullException(name, message);
        }

        if (exception is ArgumentOutOfRangeException)
        {
            return new ArgumentOutOfRangeException(name, message);
        }

        if (exception is ArgumentException)
        {
            return new ArgumentException(message, name);
        }

        return exception;
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the expression evaluates to <c>false</c>.
    /// </summary>
    /// <param name="name">The name of the parameter we are validating.</param>
    /// <param name="expression">The expression that will be evaluated.</param>
    /// <param name="message">The message associated with the <see cref="Exception"/></param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the expression evaluates to <c>false</c></exception>.
    public static void IsInRange(string name, bool expression, string message = null)
    {
        IsValid<ArgumentOutOfRangeException>(name, expression, string.IsNullOrWhiteSpace(message) ? $"The value passed for '{name}' is out of range." : message);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the value is <c>null</c>.
    /// </summary>
    /// <param name="name">The name of the parameter we are validating.</param>
    /// <param name="value">The value that will be evaluated.</param>
    /// <param name="message">The message associated with the <see cref="Exception"/></param>
    /// <exception cref="ArgumentNullException">Thrown when the value is <c>null</c></exception>.
    public static void IsNotNull<T>(string name, T value, string message = null)
    {
        IsValid<ArgumentNullException>(name, value != null, string.IsNullOrWhiteSpace(message) ? $"The value passed for '{name}' is null." : message);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the value is <c>null</c> or <c>whitespace</c>.
    /// </summary>
    /// <param name="name">The name of the parameter we are validating.</param>
    /// <param name="value">The value that will be evaluated.</param>
    /// <param name="message">The message associated with the <see cref="Exception"/></param>
    /// <exception cref="ArgumentException">Thrown when the value is <c>null</c> or <c>whitespace</c>.</exception>.
    public static void IsNotNullOrWhitespace(string name, string value, string message = null)
    {
        IsValid<ArgumentException>(name, !string.IsNullOrWhiteSpace(value), string.IsNullOrWhiteSpace(message) ? $"The value passed for '{name}' is empty, null, or whitespace." : message);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the expression evaluates to <c>false</c>.
    /// </summary>
    /// <param name="name">The name of the parameter we are validating.</param>
    /// <param name="expression">The expression that will be evaluated.</param>
    /// <param name="message">The message associated with the <see cref="Exception"/></param>
    /// <exception cref="ArgumentException">Thrown when the expression evaluates to <c>false</c></exception>.
    public static void IsValid(string name, bool expression, string message = null)
    {
        IsValid<ArgumentException>(name, expression, string.IsNullOrWhiteSpace(message) ? $"The value passed for '{name}' is not valid." : message);
    }

    /// <summary>
    /// Throws an exception if the expression evaluates to <c>false</c>.
    /// </summary>
    /// <typeparam name="TException">The type of <see cref="Exception"/> to throw.</typeparam>
    /// <param name="name">The name of the parameter we are validating.</param>
    /// <param name="expression">The expression that will be evaluated.</param>
    /// <param name="message">The message associated with the <see cref="Exception"/></param>
    public static void IsValid<TException>(string name, bool expression, string message = null) where TException : Exception, new()
    {
        if (expression)
        {
            return;
        }

        throw GetException<TException>(name, string.IsNullOrWhiteSpace(message) ? $"The value passed for '{name}' is not valid." : message);
    }
}