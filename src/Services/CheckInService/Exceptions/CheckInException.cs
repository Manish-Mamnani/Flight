namespace CheckInService.Exceptions;

public class CheckInException : Exception
{
    public CheckInException(string message) : base(message)
    {
    }
}
