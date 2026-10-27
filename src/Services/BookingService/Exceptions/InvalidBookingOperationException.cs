namespace BookingService.Exceptions;

public class InvalidBookingOperationException : Exception
{
    public InvalidBookingOperationException(string message) : base(message)
    {
    }
}
