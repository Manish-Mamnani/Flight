namespace SearchService.Exceptions;

public class FlightNotFoundException : Exception
{
    public FlightNotFoundException(string message = "Flight not found") : base(message)
    {
    }
}
