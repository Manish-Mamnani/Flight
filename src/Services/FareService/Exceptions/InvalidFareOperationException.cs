namespace FareService.Exceptions;

public class InvalidFareOperationException : Exception
{
    public InvalidFareOperationException(string message) : base(message)
    {
    }
}
