namespace Common.Exceptions;

public class InvalidDeleteException : Exception
{
    public InvalidDeleteException()
    {
    }

    public InvalidDeleteException(string message)
        : base(message)
    {
    }

    public InvalidDeleteException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
