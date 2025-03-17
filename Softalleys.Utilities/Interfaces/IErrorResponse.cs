using System;
using System.Collections.Generic;

namespace Softalleys.Utilities.Interfaces;

/// <summary>
/// Represents a contract for an error response similar to the default validation error response of HTTP 400 in ASP\.NET.
/// </summary>
public interface IErrorResponse
{
    /// <summary>
    /// Gets the error code or identifier.
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    /// Gets the error title or short description.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets a detailed error message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the HTTP status code associated with the error.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Gets a dictionary containing validation errors, where the key is the field name
    /// and the value is an array of error messages for that field.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; init; }

    /// <summary>
    /// Gets a unique identifier for tracing the error through the system.
    /// </summary>
    public string? TraceId { get; init; }
}