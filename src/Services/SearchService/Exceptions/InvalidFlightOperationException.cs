namespace SearchService.Exceptions;

public class InvalidFlightOperationException : Exception
{
    public InvalidFlightOperationException(string message) : base(message)
    {
    }
}
