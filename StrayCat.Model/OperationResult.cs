namespace StrayCat.Model
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static OperationResult<T> Ok(T data)
        {
            return new OperationResult<T> { Success = true, Data = data };
        }

        public static OperationResult<T> Fail(string message)
        {
            return new OperationResult<T> { Success = false, Message = message };
        }
    }
}
