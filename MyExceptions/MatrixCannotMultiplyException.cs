namespace RayTracerChallenge.MyExceptions;

public class MatrixCannotMultiplyException: Exception
{
    public MatrixCannotMultiplyException() : base()
    {
        
    }
    public MatrixCannotMultiplyException(string message) : base(message)
    {
        
    }
}