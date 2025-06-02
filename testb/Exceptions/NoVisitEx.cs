namespace Template.Exceptions;

public class NoVisitEx : Exception
{
    public NoVisitEx()
    {
    }

    public NoVisitEx(string? message) : base(message)
    {
    }

    public NoVisitEx(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}