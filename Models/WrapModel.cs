
namespace atmnr_api.Models;
public class WrapModel<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }

    public static WrapModel<T> Ok(T data)
    {
        return new WrapModel<T>
        {
            Success = true,
            Data = data
        };
    }
    public static WrapModel<T> Fail(string message)
    {
        return new WrapModel<T>
        {
            Success = false,
            Message = message
        };
    }
}