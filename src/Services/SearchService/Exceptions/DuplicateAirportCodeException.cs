namespace SearchService.Exceptions;

public class DuplicateAirportCodeException : Exception
{
    public DuplicateAirportCodeException(string message = "Airport code already exists") : base(message)
    {
    }
}
