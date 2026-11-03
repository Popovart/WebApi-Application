namespace Common.Models;

public enum ResponseStatus
{
    /// <summary>
    /// default value
    /// </summary>
    None,

    /// <summary>
    /// Indicates that the operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// Content was not found.
    /// </summary>
    NotFound, 

    /// <summary>
    /// Represents an error that occurs when a method call is invalid for the object's current state.
    /// </summary>
    InvalidOperation,
    
    /// <summary>
    /// Unexpected failure occurred during the operation.
    /// </summary>
    InternalError
}