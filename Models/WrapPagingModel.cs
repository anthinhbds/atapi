
namespace atmnr_api.Models;
public class WrapPagingModel<T> : WrapModel<T>
{
    public int Total { get; set; }

    public static WrapPagingModel<T> Ok(T data, int total)
    {
        return new WrapPagingModel<T>
        {
            Success = true,
            Total = total,
            Data = data
        };
    }
}