using System;

namespace EventLogger
{
    /// <summary>
    /// Provides a logging interface for generic event dispatcher components.
    /// Implement this interface to provide custom logging functionality for event processing.
    /// </summary>
    public interface IGenericEventDispatcherLogger
    {
        /// <summary>
        /// Logs a warning message indicating a potential issue or unexpected condition
        /// that does not prevent the application from continuing execution.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        void LogWarning(string message);
        
        /// <summary>
        /// Logs an error message indicating a problem that prevented the normal operation
        /// of the application or event processing.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        void LogError(string message);
        
        /// <summary>
        /// Logs an error message with an associated exception, providing detailed information
        /// about an error condition that occurred during event processing.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="exception">The exception that caused the error condition.</param>
        void LogError(string message, Exception exception);
    }
}
