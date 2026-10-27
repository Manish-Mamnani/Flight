namespace CheckInService.Exceptions;

public class SeatAlreadyAssignedException : Exception
{
    public SeatAlreadyAssignedException(string message = "Seat already assigned") : base(message)
    {
    }
}
