namespace BookingService.Exceptions;

public class BookingNotFoundException : Exception
{
    public BookingNotFoundException(string message = "Booking not found") : base(message)
    {
    }
}
