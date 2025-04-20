namespace medical.Dto.Shared
{
    public class ResultDto
    {
        public bool Success { get; protected set; } // Can only be set internally or by derived classes
        public string? ErrorMessage { get; protected set; }

        public static ResultDto Ok() => new ResultDto { Success = true };
        public static ResultDto Fail(string error) => new ResultDto { Success = false, ErrorMessage = error };
    }

    public class ResultDto<T> : ResultDto
    {
        public T? Data { get; private set; } // Make setter private

        public static ResultDto<T> Ok(T data) => new ResultDto<T> { Success = true, Data = data };
        public new static ResultDto<T> Fail(string error) => new ResultDto<T> { Success = false, ErrorMessage = error };
    }
}
