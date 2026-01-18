namespace Kyc.Aggregation.Application.Exceptions;

public abstract class ApplicationExceptionBase(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class NotFoundException(string message, Exception? innerException = null)
    : ApplicationExceptionBase(message, innerException);

public sealed class ValidationException(string message, Exception? innerException = null)
    : ApplicationExceptionBase(message, innerException);

public sealed class ExternalDependencyException(string dependencyName, string message, Exception? innerException = null)
    : ApplicationExceptionBase(message, innerException)
{
    public string DependencyName { get; } = dependencyName;
}
