namespace IdentityService.Exceptions;

public class InvalidPasswordException : Exception
{
    public InvalidPasswordException(string message = "Password does not meet requirements") : base(message)
    {
    }
}
