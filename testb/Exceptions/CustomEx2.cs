namespace Template.Exceptions;

public class CustomEx2 : Exception
{
    public CustomEx2()
    {
    }

    public CustomEx2(string? message) : base(message)
    {
    }

    public CustomEx2(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}