namespace Detector.Exceptions;

public class MatchingCaptureDeviceNotFoundException : Exception
{
    public MatchingCaptureDeviceNotFoundException()
        : base("Matching network capture device not found.")
    {
    }

    public MatchingCaptureDeviceNotFoundException(string message)
        : base(message)
    {
    }

    public MatchingCaptureDeviceNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
