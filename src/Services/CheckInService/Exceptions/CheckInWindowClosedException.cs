namespace CheckInService.Exceptions;

public class CheckInWindowClosedException : Exception
{
    public CheckInWindowClosedException(string message = "check-in not open") : base(message)
    {
    }
}
