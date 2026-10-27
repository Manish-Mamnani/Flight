namespace SearchService.Exceptions;

public class AirportNotFoundException : Exception
{
    public AirportNotFoundException(string message = "Airport not found") : base(message)
    {
    }
}
