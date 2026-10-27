namespace IdentityService.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string message = "Email already registered") : base(message)
    {
    }
}
