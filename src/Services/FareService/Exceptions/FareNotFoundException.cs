namespace FareService.Exceptions;

public class FareNotFoundException : Exception
{
    public FareNotFoundException(string message = "Fare not available") : base(message)
    {
    }
}
